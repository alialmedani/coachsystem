using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace CoachApp.Apis.NutritionPlans;

/// <summary>Trainee-facing: the signed-in trainee reads their own nutrition plans.</summary>
[Authorize(CoachAppPermissions.Trainee.MyNutritionPlans.Default)]
public class MyNutritionPlanAppService : CoachAppAppService, IMyNutritionPlanAppService
{
    private readonly IRepository<NutritionPlan, Guid> _planRepository;
    private readonly IRepository<Food, Guid> _foodRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public MyNutritionPlanAppService(
        IRepository<NutritionPlan, Guid> planRepository,
        IRepository<Food, Guid> foodRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _planRepository = planRepository;
        _foodRepository = foodRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<List<NutritionPlanDto>> GetListAsync()
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var plans = await _planRepository.GetListAsync(x => x.TraineeId == traineeId);
        return ObjectMapper.Map<List<NutritionPlan>, List<NutritionPlanDto>>(plans);
    }

    public virtual async Task<NutritionPlanDto> GetAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var plan = await _planRepository.GetAsync(id, includeDetails: true);
        if (plan.TraineeId != traineeId)
        {
            throw new EntityNotFoundException(typeof(NutritionPlan), id);
        }

        return await MapToDetailDtoAsync(plan);
    }

    public virtual async Task<NutritionPlanDto?> GetActiveAsync()
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

    private async Task<NutritionPlanDto> MapToDetailDtoAsync(NutritionPlan plan)
    {
        var dto = ObjectMapper.Map<NutritionPlan, NutritionPlanDto>(plan);
        await NutritionPlanEnricher.EnrichAsync(dto, _foodRepository);
        return dto;
    }
}
