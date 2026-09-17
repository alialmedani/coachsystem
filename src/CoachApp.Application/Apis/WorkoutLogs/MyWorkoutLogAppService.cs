using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.WorkoutLogs;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own sessions.</summary>
[Authorize(CoachAppPermissions.Trainee.WorkoutLogs.Default)]
public class MyWorkoutLogAppService : MyTraineeAppServiceBase, IMyWorkoutLogAppService
{
    private readonly IRepository<WorkoutLog, Guid> _logRepository;
    private readonly IRepository<Exercise, Guid> _exerciseRepository;
    private readonly IRepository<WorkoutPlan, Guid> _planRepository;

    public MyWorkoutLogAppService(
        IRepository<WorkoutLog, Guid> logRepository,
        IRepository<Exercise, Guid> exerciseRepository,
        IRepository<WorkoutPlan, Guid> planRepository)
    {
        _logRepository = logRepository;
        _exerciseRepository = exerciseRepository;
        _planRepository = planRepository;
    }

    [Authorize(CoachAppPermissions.Trainee.WorkoutLogs.Create)]
    public virtual async Task<WorkoutLogDto> CreateAsync(CreateWorkoutLogDto input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        await ValidatePlanReferencesAsync(traineeId, input.WorkoutPlanId, input.WorkoutDayId);
        await CheckExercisesExistAsync(input.Entries.Select(e => e.ExerciseId));

        var log = WorkoutLog.Create(
            GuidGenerator.Create(),
            traineeId,
            input.Date,
            input.WorkoutPlanId,
            input.WorkoutDayId,
            input.Notes,
            CurrentTenant.Id);

        foreach (var entry in input.Entries.OrderBy(e => e.Order))
        {
            log.AddEntry(GuidGenerator.Create(), entry.ExerciseId, entry.Order, entry.Sets, entry.Reps, entry.WeightKg, entry.Notes);
        }

        await _logRepository.InsertAsync(log, autoSave: true);
        return await MapDetailAsync(log);
    }

    [Authorize(CoachAppPermissions.Trainee.WorkoutLogs.Create)]
    public virtual async Task<WorkoutLogDto> CreateFromDayAsync(CreateWorkoutLogFromDayDto input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        // Resolve the day through its aggregate root, scoped to the current tenant (the plan repo is
        // IMultiTenant) AND to the current trainee. A day from another tenant/trainee (or unknown) is
        // reported uniformly as not-found — we never touch the non-multitenant child table directly and
        // never leak the parent plan's id.
        var planQuery = await _planRepository.GetQueryableAsync();
        var owned = await AsyncExecuter.FirstOrDefaultAsync(
            planQuery.Where(p => p.TraineeId == traineeId && p.Days.Any(d => d.Id == input.WorkoutDayId)));
        if (owned == null)
        {
            throw new EntityNotFoundException(typeof(WorkoutDay), input.WorkoutDayId);
        }

        var plan = await _planRepository.GetAsync(owned.Id, includeDetails: true);
        var planDay = plan.Days.First(d => d.Id == input.WorkoutDayId);

        var log = WorkoutLog.Create(
            GuidGenerator.Create(),
            traineeId,
            input.Date,
            plan.Id,
            planDay.Id,
            input.Notes,
            CurrentTenant.Id);

        // Snapshot the prescribed exercises into the log: the plan values seed both the prescribed
        // fields (durable record) and the initial actuals (which the trainee can later adjust).
        foreach (var ex in planDay.Exercises.OrderBy(e => e.Order))
        {
            log.AddEntry(
                GuidGenerator.Create(),
                ex.ExerciseId,
                ex.Order,
                ex.Sets,
                ex.Reps,
                ex.WeightKg,
                ex.Notes,
                prescribedSets: ex.Sets,
                prescribedReps: ex.Reps,
                prescribedWeightKg: ex.WeightKg);
        }

        await _logRepository.InsertAsync(log, autoSave: true);
        return await MapDetailAsync(log);
    }

    public virtual async Task<PagedResultDto<WorkoutLogDto>> GetListAsync(GetMyWorkoutLogListInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        var query = (await _logRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == traineeId)
            .WhereIf(input.FromDate.HasValue, x => x.Date >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue, x => x.Date <= input.ToDate!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(WorkoutLog.Date)} desc" : input.Sorting;
        var logs = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<WorkoutLogDto>(totalCount, ObjectMapper.Map<List<WorkoutLog>, List<WorkoutLogDto>>(logs));
    }

    public virtual async Task<WorkoutLogDto> GetAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var log = await GetOwnedWithDetailsAsync(id, traineeId);
        return await MapDetailAsync(log);
    }

    [Authorize(CoachAppPermissions.Trainee.WorkoutLogs.Update)]
    public virtual async Task<WorkoutLogDto> UpdateAsync(Guid id, UpdateWorkoutLogDto input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var log = await GetOwnedWithDetailsAsync(id, traineeId);
        await CheckExercisesExistAsync(input.Entries.Select(e => e.ExerciseId));

        // The prescribed snapshot is server-owned: capture it before the rebuild (keyed by the
        // entry's stable identity, ExerciseId + Order) and restore it, so client input can never
        // overwrite it. Entries with no prior match (newly added) carry no prescribed values.
        var prescribed = new Dictionary<(Guid ExerciseId, int Order), (int? Sets, string? Reps, decimal? WeightKg)>();
        foreach (var existing in log.Entries)
        {
            prescribed[(existing.ExerciseId, existing.Order)] =
                (existing.PrescribedSets, existing.PrescribedReps, existing.PrescribedWeightKg);
        }

        log.Date = input.Date;
        log.Notes = input.Notes;

        // Full replace of entries (mirrors the plan clear-and-rebuild convention).
        log.ClearEntries();
        foreach (var entry in input.Entries.OrderBy(e => e.Order))
        {
            prescribed.TryGetValue((entry.ExerciseId, entry.Order), out var snapshot);
            log.AddEntry(
                GuidGenerator.Create(),
                entry.ExerciseId,
                entry.Order,
                entry.Sets,
                entry.Reps,
                entry.WeightKg,
                entry.Notes,
                snapshot.Sets,
                snapshot.Reps,
                snapshot.WeightKg);
        }

        await _logRepository.UpdateAsync(log, autoSave: true);
        return await MapDetailAsync(log);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var log = await GetOwnedWithDetailsAsync(id, traineeId);
        await _logRepository.DeleteAsync(log, autoSave: true);
    }

    private async Task<WorkoutLog> GetOwnedWithDetailsAsync(Guid id, Guid traineeId)
    {
        var query = await _logRepository.WithDetailsAsync(x => x.Entries);
        var log = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TraineeId == traineeId));
        if (log == null)
        {
            throw new EntityNotFoundException(typeof(WorkoutLog), id);
        }

        return log;
    }

    /// <summary>
    /// Validates any plan/day reference a trainee attaches to a manual log actually belongs to them.
    /// A foreign or unknown plan/day (or a day that isn't part of the referenced plan) is reported
    /// uniformly as not-found — the same convention the other My* services use — so ownership of
    /// another trainee's data is never disclosed. Omitted references (a manual log) pass through.
    /// </summary>
    private async Task ValidatePlanReferencesAsync(Guid traineeId, Guid? workoutPlanId, Guid? workoutDayId)
    {
        if (workoutPlanId == null && workoutDayId == null)
        {
            return;
        }

        var planQuery = await _planRepository.GetQueryableAsync();

        if (workoutDayId.HasValue)
        {
            // The day must belong to a plan owned by the trainee; when a plan id is also supplied it
            // must be that same plan, so a day from a different plan can't be attached.
            var owned = await AsyncExecuter.FirstOrDefaultAsync(
                planQuery.Where(p => p.TraineeId == traineeId
                    && (workoutPlanId == null || p.Id == workoutPlanId.Value)
                    && p.Days.Any(d => d.Id == workoutDayId.Value)));
            if (owned == null)
            {
                throw new EntityNotFoundException(typeof(WorkoutDay), workoutDayId.Value);
            }
        }
        else
        {
            var owned = await AsyncExecuter.FirstOrDefaultAsync(
                planQuery.Where(p => p.TraineeId == traineeId && p.Id == workoutPlanId!.Value));
            if (owned == null)
            {
                throw new EntityNotFoundException(typeof(WorkoutPlan), workoutPlanId!.Value);
            }
        }
    }

    private async Task CheckExercisesExistAsync(IEnumerable<Guid> exerciseIds)
    {
        var ids = exerciseIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var found = await _exerciseRepository.CountAsync(x => ids.Contains(x.Id));
        if (found != ids.Count)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.ExercisesNotFound);
        }
    }

    private async Task<WorkoutLogDto> MapDetailAsync(WorkoutLog log)
    {
        var dto = ObjectMapper.Map<WorkoutLog, WorkoutLogDto>(log);
        await WorkoutLogEnricher.EnrichAsync(dto, _exerciseRepository);
        return dto;
    }
}
