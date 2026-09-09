using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.WorkoutPlans;

/// <summary>Trainee-facing: the signed-in trainee reads their own workout plans.</summary>
[Authorize(CoachAppPermissions.Trainee.MyWorkoutPlans.Default)]
public class MyWorkoutPlanAppService : MyTraineeAppServiceBase, IMyWorkoutPlanAppService
{
    private readonly IRepository<WorkoutPlan, Guid> _planRepository;
    private readonly IRepository<Exercise, Guid> _exerciseRepository;

    public MyWorkoutPlanAppService(
        IRepository<WorkoutPlan, Guid> planRepository,
        IRepository<Exercise, Guid> exerciseRepository)
    {
        _planRepository = planRepository;
        _exerciseRepository = exerciseRepository;
    }

    public virtual async Task<List<WorkoutPlanDto>> GetListAsync()
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var plans = await _planRepository.GetListAsync(x => x.TraineeId == traineeId);
        return ObjectMapper.Map<List<WorkoutPlan>, List<WorkoutPlanDto>>(plans);
    }

    public virtual async Task<WorkoutPlanDto> GetAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var plan = await _planRepository.GetAsync(id, includeDetails: true);
        if (plan.TraineeId != traineeId)
        {
            throw new EntityNotFoundException(typeof(WorkoutPlan), id);
        }

        return await MapToDetailDtoAsync(plan);
    }

    public virtual async Task<WorkoutPlanDto?> GetActiveAsync()
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var active = await _planRepository.FirstOrDefaultAsync(x => x.TraineeId == traineeId && x.IsActive);
        if (active == null)
        {
            return null;
        }

        var plan = await _planRepository.GetAsync(active.Id, includeDetails: true);
        return await MapToDetailDtoAsync(plan);
    }

    private async Task<WorkoutPlanDto> MapToDetailDtoAsync(WorkoutPlan plan)
    {
        var dto = ObjectMapper.Map<WorkoutPlan, WorkoutPlanDto>(plan);

        var ids = dto.Days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId).Distinct().ToList();
        if (ids.Count > 0)
        {
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

        return dto;
    }
}
