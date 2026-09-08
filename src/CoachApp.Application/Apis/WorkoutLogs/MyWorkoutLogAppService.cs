using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace CoachApp.Apis.WorkoutLogs;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own sessions.</summary>
[Authorize(CoachAppPermissions.Trainee.WorkoutLogs.Default)]
public class MyWorkoutLogAppService : CoachAppAppService, IMyWorkoutLogAppService
{
    private readonly IRepository<WorkoutLog, Guid> _logRepository;
    private readonly IRepository<Exercise, Guid> _exerciseRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public MyWorkoutLogAppService(
        IRepository<WorkoutLog, Guid> logRepository,
        IRepository<Exercise, Guid> exerciseRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _logRepository = logRepository;
        _exerciseRepository = exerciseRepository;
        _traineeRepository = traineeRepository;
    }

    [Authorize(CoachAppPermissions.Trainee.WorkoutLogs.Create)]
    public virtual async Task<WorkoutLogDto> CreateAsync(CreateWorkoutLogDto input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
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

    private async Task<Guid> GetCurrentTraineeIdAsync()
    {
        var userId = CurrentUser.GetId();
        var trainee = await _traineeRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (trainee == null)
        {
            throw new EntityNotFoundException(typeof(Trainee), userId);
        }

        return trainee.Id;
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
            throw new UserFriendlyException(L["OneOrMoreExercisesDoNotExist"]);
        }
    }

    private async Task<WorkoutLogDto> MapDetailAsync(WorkoutLog log)
    {
        var dto = ObjectMapper.Map<WorkoutLog, WorkoutLogDto>(log);
        await WorkoutLogEnricher.EnrichAsync(dto, _exerciseRepository);
        return dto;
    }
}
