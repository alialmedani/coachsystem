using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.WorkoutPlans;

/// <summary>Coach-facing management of trainees' workout plans (Plan → Days → Exercises).</summary>
[Authorize(CoachAppPermissions.Coach.WorkoutPlans.Default)]
public class WorkoutPlanAppService : CoachAppAppService, IWorkoutPlanAppService
{
    private readonly IRepository<WorkoutPlan, Guid> _planRepository;
    private readonly IRepository<Exercise, Guid> _exerciseRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public WorkoutPlanAppService(
        IRepository<WorkoutPlan, Guid> planRepository,
        IRepository<Exercise, Guid> exerciseRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _planRepository = planRepository;
        _exerciseRepository = exerciseRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<WorkoutPlanDto> GetAsync(Guid id)
    {
        var plan = await _planRepository.GetAsync(id, includeDetails: true);
        return await MapToDetailDtoAsync(plan);
    }

    public virtual async Task<PagedResultDto<WorkoutPlanDto>> GetListAsync(GetWorkoutPlanListInput input)
    {
        var query = await _planRepository.GetQueryableAsync();

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!))
            .WhereIf(input.TraineeId.HasValue, x => x.TraineeId == input.TraineeId!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(WorkoutPlan.Name)} asc" : input.Sorting;

        var plans = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Summaries — days are intentionally not loaded here.
        return new PagedResultDto<WorkoutPlanDto>(
            totalCount,
            ObjectMapper.Map<List<WorkoutPlan>, List<WorkoutPlanDto>>(plans));
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlans.Create)]
    public virtual async Task<WorkoutPlanDto> CreateAsync(CreateUpdateWorkoutPlanDto input)
    {
        await CheckTraineeExistsAsync(input.TraineeId);
        await CheckExercisesExistAsync(input);

        var plan = WorkoutPlan.Create(GuidGenerator.Create(), input.TraineeId, input.Name, input.Description, CurrentTenant.Id);
        BuildDays(plan, input);

        await _planRepository.InsertAsync(plan, autoSave: true);

        if (input.IsActive)
        {
            await SetActiveInternalAsync(plan);
        }

        return await MapToDetailDtoAsync(plan);
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlans.Update)]
    public virtual async Task<WorkoutPlanDto> UpdateAsync(Guid id, CreateUpdateWorkoutPlanDto input)
    {
        var plan = await _planRepository.GetAsync(id, includeDetails: true);

        if (plan.TraineeId != input.TraineeId)
        {
            await CheckTraineeExistsAsync(input.TraineeId);
            plan.TraineeId = input.TraineeId;
        }

        await CheckExercisesExistAsync(input);

        plan.Name = input.Name;
        plan.Description = input.Description;

        // Full replace of the day/exercise structure.
        plan.ClearDays();
        BuildDays(plan, input);

        await _planRepository.UpdateAsync(plan, autoSave: true);

        if (input.IsActive && !plan.IsActive)
        {
            await SetActiveInternalAsync(plan);
        }
        else if (!input.IsActive && plan.IsActive)
        {
            plan.Deactivate();
            await _planRepository.UpdateAsync(plan, autoSave: true);
        }

        return await MapToDetailDtoAsync(plan);
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlans.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _planRepository.DeleteAsync(id);
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlans.Update)]
    public virtual async Task<WorkoutPlanDto> SetActiveAsync(Guid id)
    {
        var plan = await _planRepository.GetAsync(id, includeDetails: true);
        await SetActiveInternalAsync(plan);
        return await MapToDetailDtoAsync(plan);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private void BuildDays(WorkoutPlan plan, CreateUpdateWorkoutPlanDto input)
    {
        foreach (var dayDto in input.Days.OrderBy(d => d.Order))
        {
            var day = plan.AddDay(GuidGenerator.Create(), dayDto.Name, dayDto.Order, dayDto.ScheduledDay);
            foreach (var exDto in dayDto.Exercises.OrderBy(e => e.Order))
            {
                day.AddExercise(
                    GuidGenerator.Create(),
                    exDto.ExerciseId,
                    exDto.Order,
                    exDto.Sets,
                    exDto.Reps,
                    exDto.WeightKg,
                    exDto.RestSeconds,
                    exDto.Notes);
            }
        }
    }

    private async Task SetActiveInternalAsync(WorkoutPlan plan)
    {
        var others = await _planRepository.GetListAsync(
            x => x.TraineeId == plan.TraineeId && x.Id != plan.Id && x.IsActive);
        foreach (var other in others)
        {
            other.Deactivate();
            await _planRepository.UpdateAsync(other);
        }

        plan.Activate();
        await _planRepository.UpdateAsync(plan, autoSave: true);
    }

    private async Task CheckTraineeExistsAsync(Guid traineeId)
    {
        var trainee = await _traineeRepository.FindAsync(traineeId);
        if (trainee == null)
        {
            throw new UserFriendlyException(L["TheSelectedTraineeDoesNotExist"]);
        }
    }

    private async Task CheckExercisesExistAsync(CreateUpdateWorkoutPlanDto input)
    {
        var ids = input.Days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId).Distinct().ToList();
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

    private async Task<WorkoutPlanDto> MapToDetailDtoAsync(WorkoutPlan plan)
    {
        var dto = ObjectMapper.Map<WorkoutPlan, WorkoutPlanDto>(plan);
        await EnrichExerciseNamesAsync(dto);
        return dto;
    }

    private async Task EnrichExerciseNamesAsync(WorkoutPlanDto dto)
    {
        var ids = dto.Days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId).Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var names = (await _exerciseRepository.GetListAsync(x => ids.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => x.Name);

        foreach (var day in dto.Days)
        {
            foreach (var ex in day.Exercises)
            {
                if (names.TryGetValue(ex.ExerciseId, out var name))
                {
                    ex.ExerciseName = name;
                }
            }
        }
    }
}
