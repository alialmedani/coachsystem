using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Entites.WorkoutPlanTemplates;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.WorkoutPlanTemplates;

/// <summary>
/// Coach-facing workout plan template library. Templates are tenant-scoped blueprints with no
/// trainee/active state. Two conversions bridge templates and real plans:
/// <see cref="SaveAsTemplateAsync"/> (plan → template) and <see cref="CloneToTraineeAsync"/>
/// (template → a new, inactive <see cref="WorkoutPlan"/> the coach activates separately).
/// </summary>
[Authorize(CoachAppPermissions.Coach.WorkoutPlanTemplates.Default)]
public class WorkoutPlanTemplateAppService : CoachAppAppService, IWorkoutPlanTemplateAppService
{
    private readonly IRepository<WorkoutPlanTemplate, Guid> _templateRepository;
    private readonly IRepository<WorkoutPlan, Guid> _planRepository;
    private readonly IRepository<Exercise, Guid> _exerciseRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public WorkoutPlanTemplateAppService(
        IRepository<WorkoutPlanTemplate, Guid> templateRepository,
        IRepository<WorkoutPlan, Guid> planRepository,
        IRepository<Exercise, Guid> exerciseRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _templateRepository = templateRepository;
        _planRepository = planRepository;
        _exerciseRepository = exerciseRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<WorkoutPlanTemplateDto> GetAsync(Guid id)
    {
        var template = await _templateRepository.GetAsync(id, includeDetails: true);
        return await MapToDetailDtoAsync(template);
    }

    public virtual async Task<PagedResultDto<WorkoutPlanTemplateDto>> GetListAsync(GetWorkoutPlanTemplateListInput input)
    {
        var query = await _templateRepository.GetQueryableAsync();

        query = query.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(WorkoutPlanTemplate.Name)} asc" : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Summaries — days are intentionally not loaded here.
        return new PagedResultDto<WorkoutPlanTemplateDto>(
            totalCount,
            ObjectMapper.Map<List<WorkoutPlanTemplate>, List<WorkoutPlanTemplateDto>>(items));
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlanTemplates.Create)]
    public virtual async Task<WorkoutPlanTemplateDto> CreateAsync(CreateUpdateWorkoutPlanTemplateDto input)
    {
        await CheckExercisesExistAsync(GetExerciseIds(input));

        var template = WorkoutPlanTemplate.Create(GuidGenerator.Create(), input.Name, input.Description, CurrentTenant.Id);
        BuildDays(template, input);

        await _templateRepository.InsertAsync(template, autoSave: true);
        return await MapToDetailDtoAsync(template);
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlanTemplates.Update)]
    public virtual async Task<WorkoutPlanTemplateDto> UpdateAsync(Guid id, CreateUpdateWorkoutPlanTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(id, includeDetails: true);

        await CheckExercisesExistAsync(GetExerciseIds(input));

        template.Name = input.Name;
        template.Description = input.Description;

        // Full replace of the day/exercise structure.
        template.ClearDays();
        BuildDays(template, input);

        await _templateRepository.UpdateAsync(template, autoSave: true);
        return await MapToDetailDtoAsync(template);
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlanTemplates.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _templateRepository.DeleteAsync(id);
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlans.Create)]
    public virtual async Task<WorkoutPlanDto> CloneToTraineeAsync(Guid id, CloneWorkoutTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(id, includeDetails: true);

        await CheckTraineeExistsAsync(input.TraineeId);
        await CheckExercisesExistAsync(template.Days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId));

        var plan = WorkoutPlan.Create(
            GuidGenerator.Create(),
            input.TraineeId,
            string.IsNullOrWhiteSpace(input.Name) ? template.Name : input.Name!,
            input.Description ?? template.Description,
            CurrentTenant.Id);

        foreach (var day in template.Days.OrderBy(d => d.Order))
        {
            var planDay = plan.AddDay(GuidGenerator.Create(), day.Name, day.Order);
            foreach (var ex in day.Exercises.OrderBy(e => e.Order))
            {
                planDay.AddExercise(GuidGenerator.Create(), ex.ExerciseId, ex.Order, ex.Sets, ex.Reps, ex.WeightKg, ex.RestSeconds, ex.Notes);
            }
        }

        // Created inactive; the coach activates it via WorkoutPlanAppService.SetActiveAsync.
        await _planRepository.InsertAsync(plan, autoSave: true);
        return await MapPlanToDetailDtoAsync(plan);
    }

    [Authorize(CoachAppPermissions.Coach.WorkoutPlanTemplates.Create)]
    public virtual async Task<WorkoutPlanTemplateDto> SaveAsTemplateAsync(SaveWorkoutPlanAsTemplateDto input)
    {
        var plan = await _planRepository.GetAsync(input.WorkoutPlanId, includeDetails: true);

        var template = WorkoutPlanTemplate.Create(GuidGenerator.Create(), input.Name, input.Description, CurrentTenant.Id);

        foreach (var day in plan.Days.OrderBy(d => d.Order))
        {
            var templateDay = template.AddDay(GuidGenerator.Create(), day.Name, day.Order);
            foreach (var ex in day.Exercises.OrderBy(e => e.Order))
            {
                templateDay.AddExercise(GuidGenerator.Create(), ex.ExerciseId, ex.Order, ex.Sets, ex.Reps, ex.WeightKg, ex.RestSeconds, ex.Notes);
            }
        }

        await _templateRepository.InsertAsync(template, autoSave: true);
        return await MapToDetailDtoAsync(template);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private void BuildDays(WorkoutPlanTemplate template, CreateUpdateWorkoutPlanTemplateDto input)
    {
        foreach (var dayDto in input.Days.OrderBy(d => d.Order))
        {
            var day = template.AddDay(GuidGenerator.Create(), dayDto.Name, dayDto.Order);
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

    private static List<Guid> GetExerciseIds(CreateUpdateWorkoutPlanTemplateDto input)
        => input.Days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId).Distinct().ToList();

    private async Task CheckTraineeExistsAsync(Guid traineeId)
    {
        var trainee = await _traineeRepository.FindAsync(traineeId);
        if (trainee == null)
        {
            throw new UserFriendlyException(L["TheSelectedTraineeDoesNotExist"]);
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
            throw new UserFriendlyException(L["OneOrMoreExercisesDoNotExist"]);
        }
    }

    private async Task<WorkoutPlanTemplateDto> MapToDetailDtoAsync(WorkoutPlanTemplate template)
    {
        var dto = ObjectMapper.Map<WorkoutPlanTemplate, WorkoutPlanTemplateDto>(template);
        var names = await GetExerciseNamesAsync(dto.Days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId));
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

        return dto;
    }

    private async Task<WorkoutPlanDto> MapPlanToDetailDtoAsync(WorkoutPlan plan)
    {
        var dto = ObjectMapper.Map<WorkoutPlan, WorkoutPlanDto>(plan);
        var names = await GetExerciseNamesAsync(dto.Days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId));
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

        return dto;
    }

    private async Task<Dictionary<Guid, string>> GetExerciseNamesAsync(IEnumerable<Guid> exerciseIds)
    {
        var ids = exerciseIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return (await _exerciseRepository.GetListAsync(x => ids.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => x.Name);
    }
}
