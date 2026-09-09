using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Apis.NutritionPlans;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.NutritionPlanTemplates;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.NutritionPlanTemplates;

/// <summary>
/// Coach-facing nutrition plan template library. Templates are tenant-scoped blueprints with no
/// trainee/active state. Two conversions bridge templates and real plans:
/// <see cref="SaveAsTemplateAsync"/> (plan → template) and <see cref="CloneToTraineeAsync"/>
/// (template → a new, inactive <see cref="NutritionPlan"/> the coach activates separately).
/// </summary>
[Authorize(CoachAppPermissions.Coach.NutritionPlanTemplates.Default)]
public class NutritionPlanTemplateAppService : CoachAppAppService, INutritionPlanTemplateAppService
{
    private readonly IRepository<NutritionPlanTemplate, Guid> _templateRepository;
    private readonly IRepository<NutritionPlan, Guid> _planRepository;
    private readonly IRepository<Food, Guid> _foodRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public NutritionPlanTemplateAppService(
        IRepository<NutritionPlanTemplate, Guid> templateRepository,
        IRepository<NutritionPlan, Guid> planRepository,
        IRepository<Food, Guid> foodRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _templateRepository = templateRepository;
        _planRepository = planRepository;
        _foodRepository = foodRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<NutritionPlanTemplateDto> GetAsync(Guid id)
    {
        var template = await _templateRepository.GetAsync(id, includeDetails: true);
        return await MapToDetailDtoAsync(template);
    }

    public virtual async Task<PagedResultDto<NutritionPlanTemplateDto>> GetListAsync(GetNutritionPlanTemplateListInput input)
    {
        var query = await _templateRepository.GetQueryableAsync();

        query = query.WhereIf(!string.IsNullOrWhiteSpace(input.Filter), x => x.Name.Contains(input.Filter!));

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(NutritionPlanTemplate.Name)} asc" : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        // Summaries — meals are intentionally not loaded here.
        return new PagedResultDto<NutritionPlanTemplateDto>(
            totalCount,
            ObjectMapper.Map<List<NutritionPlanTemplate>, List<NutritionPlanTemplateDto>>(items));
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlanTemplates.Create)]
    public virtual async Task<NutritionPlanTemplateDto> CreateAsync(CreateUpdateNutritionPlanTemplateDto input)
    {
        await CheckFoodsExistAsync(GetFoodIds(input));

        var template = NutritionPlanTemplate.Create(GuidGenerator.Create(), input.Name, input.Description, CurrentTenant.Id);
        ApplyTargets(template, input);
        BuildMeals(template, input);

        await _templateRepository.InsertAsync(template, autoSave: true);
        return await MapToDetailDtoAsync(template);
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlanTemplates.Update)]
    public virtual async Task<NutritionPlanTemplateDto> UpdateAsync(Guid id, CreateUpdateNutritionPlanTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(id, includeDetails: true);

        await CheckFoodsExistAsync(GetFoodIds(input));

        template.Name = input.Name;
        template.Description = input.Description;
        ApplyTargets(template, input);

        // Full replace of the meal/item structure.
        template.ClearMeals();
        BuildMeals(template, input);

        await _templateRepository.UpdateAsync(template, autoSave: true);
        return await MapToDetailDtoAsync(template);
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlanTemplates.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _templateRepository.DeleteAsync(id);
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlans.Create)]
    public virtual async Task<NutritionPlanDto> CloneToTraineeAsync(Guid id, CloneNutritionTemplateDto input)
    {
        var template = await _templateRepository.GetAsync(id, includeDetails: true);

        await CheckTraineeExistsAsync(input.TraineeId);
        await CheckFoodsExistAsync(template.Meals.SelectMany(m => m.Items).Select(i => i.FoodId));

        var plan = NutritionPlan.Create(
            GuidGenerator.Create(),
            input.TraineeId,
            string.IsNullOrWhiteSpace(input.Name) ? template.Name : input.Name!,
            input.Description ?? template.Description,
            CurrentTenant.Id);

        plan.TargetCalories = template.TargetCalories;
        plan.TargetProteinG = template.TargetProteinG;
        plan.TargetCarbsG = template.TargetCarbsG;
        plan.TargetFatG = template.TargetFatG;

        foreach (var meal in template.Meals.OrderBy(m => m.Order))
        {
            var planMeal = plan.AddMeal(GuidGenerator.Create(), meal.Name, meal.Order);
            foreach (var item in meal.Items.OrderBy(i => i.Order))
            {
                planMeal.AddItem(GuidGenerator.Create(), item.FoodId, item.Order, item.Quantity);
            }
        }

        // Created inactive; the coach activates it via NutritionPlanAppService.SetActiveAsync.
        await _planRepository.InsertAsync(plan, autoSave: true);

        var dto = ObjectMapper.Map<NutritionPlan, NutritionPlanDto>(plan);
        await NutritionPlanEnricher.EnrichAsync(dto, _foodRepository);
        return dto;
    }

    [Authorize(CoachAppPermissions.Coach.NutritionPlanTemplates.Create)]
    public virtual async Task<NutritionPlanTemplateDto> SaveAsTemplateAsync(SaveNutritionPlanAsTemplateDto input)
    {
        var plan = await _planRepository.GetAsync(input.NutritionPlanId, includeDetails: true);

        var template = NutritionPlanTemplate.Create(GuidGenerator.Create(), input.Name, input.Description, CurrentTenant.Id);

        template.TargetCalories = plan.TargetCalories;
        template.TargetProteinG = plan.TargetProteinG;
        template.TargetCarbsG = plan.TargetCarbsG;
        template.TargetFatG = plan.TargetFatG;

        foreach (var meal in plan.Meals.OrderBy(m => m.Order))
        {
            var templateMeal = template.AddMeal(GuidGenerator.Create(), meal.Name, meal.Order);
            foreach (var item in meal.Items.OrderBy(i => i.Order))
            {
                templateMeal.AddItem(GuidGenerator.Create(), item.FoodId, item.Order, item.Quantity);
            }
        }

        await _templateRepository.InsertAsync(template, autoSave: true);
        return await MapToDetailDtoAsync(template);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static void ApplyTargets(NutritionPlanTemplate template, CreateUpdateNutritionPlanTemplateDto input)
    {
        template.TargetCalories = input.TargetCalories;
        template.TargetProteinG = input.TargetProteinG;
        template.TargetCarbsG = input.TargetCarbsG;
        template.TargetFatG = input.TargetFatG;
    }

    private void BuildMeals(NutritionPlanTemplate template, CreateUpdateNutritionPlanTemplateDto input)
    {
        foreach (var mealDto in input.Meals.OrderBy(m => m.Order))
        {
            var meal = template.AddMeal(GuidGenerator.Create(), mealDto.Name, mealDto.Order);
            foreach (var itemDto in mealDto.Items.OrderBy(i => i.Order))
            {
                meal.AddItem(GuidGenerator.Create(), itemDto.FoodId, itemDto.Order, itemDto.Quantity);
            }
        }
    }

    private static List<Guid> GetFoodIds(CreateUpdateNutritionPlanTemplateDto input)
        => input.Meals.SelectMany(m => m.Items).Select(i => i.FoodId).Distinct().ToList();

    private async Task CheckTraineeExistsAsync(Guid traineeId)
    {
        var trainee = await _traineeRepository.FindAsync(traineeId);
        if (trainee == null)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.TraineeNotFound);
        }
    }

    private async Task CheckFoodsExistAsync(IEnumerable<Guid> foodIds)
    {
        var ids = foodIds.Distinct().ToList();
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

    private async Task<NutritionPlanTemplateDto> MapToDetailDtoAsync(NutritionPlanTemplate template)
    {
        var dto = ObjectMapper.Map<NutritionPlanTemplate, NutritionPlanTemplateDto>(template);
        await NutritionPlanTemplateEnricher.EnrichAsync(dto, _foodRepository);
        return dto;
    }
}
