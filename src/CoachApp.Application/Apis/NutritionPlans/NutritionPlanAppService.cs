using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.NutritionPlans;

/// <summary>Coach-facing management of trainees' nutrition plans (Plan → Meals → Items).</summary>
[Authorize(CoachAppPermissions.Coach.NutritionPlans.Default)]
public class NutritionPlanAppService : CoachAppAppService, INutritionPlanAppService
{
    private readonly IRepository<NutritionPlan, Guid> _planRepository;
    private readonly IRepository<Food, Guid> _foodRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public NutritionPlanAppService(
        IRepository<NutritionPlan, Guid> planRepository,
        IRepository<Food, Guid> foodRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _planRepository = planRepository;
        _foodRepository = foodRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<NutritionPlanDto> GetAsync(Guid id)
    {
        var plan = await _planRepository.GetAsync(id, includeDetails: true);
        return await MapToDetailDtoAsync(plan);
    }

    public virtual async Task<PagedResultDto<NutritionPlanDto>> GetListAsync(GetNutritionPlanListInput input)
    {
        var query = await _planRepository.GetQueryableAsync();

        query = query
            .WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!))
            .WhereIf(input.TraineeId.HasValue, x => x.TraineeId == input.TraineeId!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(NutritionPlan.Name)} asc" : input.Sorting;
        var plans = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<NutritionPlanDto>(
            totalCount,
            ObjectMapper.Map<List<NutritionPlan>, List<NutritionPlanDto>>(plans));
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlans.Create)]
    public virtual async Task<NutritionPlanDto> CreateAsync(CreateUpdateNutritionPlanDto input)
    {
        await CheckTraineeExistsAsync(input.TraineeId);
        await CheckFoodsExistAsync(input);

        var plan = NutritionPlan.Create(GuidGenerator.Create(), input.TraineeId, input.Name, input.Description, CurrentTenant.Id);
        ApplyTargets(plan, input);
        BuildMeals(plan, input);

        await _planRepository.InsertAsync(plan, autoSave: true);

        if (input.IsActive)
        {
            await SetActiveInternalAsync(plan);
        }

        return await MapToDetailDtoAsync(plan);
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlans.Update)]
    public virtual async Task<NutritionPlanDto> UpdateAsync(Guid id, CreateUpdateNutritionPlanDto input)
    {
        var plan = await _planRepository.GetAsync(id, includeDetails: true);

        if (plan.TraineeId != input.TraineeId)
        {
            await CheckTraineeExistsAsync(input.TraineeId);
            plan.TraineeId = input.TraineeId;
        }

        await CheckFoodsExistAsync(input);

        plan.Name = input.Name;
        plan.Description = input.Description;
        ApplyTargets(plan, input);

        plan.ClearMeals();
        BuildMeals(plan, input);

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

    [Authorize(CoachAppPermissions.Coach.NutritionPlans.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _planRepository.DeleteAsync(id);
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlans.Update)]
    public virtual async Task<NutritionPlanDto> SetActiveAsync(Guid id)
    {
        var plan = await _planRepository.GetAsync(id, includeDetails: true);
        await SetActiveInternalAsync(plan);
        return await MapToDetailDtoAsync(plan);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static void ApplyTargets(NutritionPlan plan, CreateUpdateNutritionPlanDto input)
    {
        plan.TargetCalories = input.TargetCalories;
        plan.TargetProteinG = input.TargetProteinG;
        plan.TargetCarbsG = input.TargetCarbsG;
        plan.TargetFatG = input.TargetFatG;
    }

    private void BuildMeals(NutritionPlan plan, CreateUpdateNutritionPlanDto input)
    {
        foreach (var mealDto in input.Meals.OrderBy(m => m.Order))
        {
            var meal = plan.AddMeal(GuidGenerator.Create(), mealDto.Name, mealDto.Order);
            foreach (var itemDto in mealDto.Items.OrderBy(i => i.Order))
            {
                meal.AddItem(GuidGenerator.Create(), itemDto.FoodId, itemDto.Order, itemDto.Quantity);
            }
        }
    }

    private async Task SetActiveInternalAsync(NutritionPlan plan)
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
            throw new BusinessException(CoachAppDomainErrorCodes.TraineeNotFound);
        }
    }

    private async Task CheckFoodsExistAsync(CreateUpdateNutritionPlanDto input)
    {
        var ids = input.Meals.SelectMany(m => m.Items).Select(i => i.FoodId).Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var found = await _foodRepository.CountAsync(x => ids.Contains(x.Id));
        if (found != ids.Count)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.FoodsNotFound);
        }
    }

    private async Task<NutritionPlanDto> MapToDetailDtoAsync(NutritionPlan plan)
    {
        var dto = ObjectMapper.Map<NutritionPlan, NutritionPlanDto>(plan);
        await NutritionPlanEnricher.EnrichAsync(dto, _foodRepository);
        return dto;
    }
}
