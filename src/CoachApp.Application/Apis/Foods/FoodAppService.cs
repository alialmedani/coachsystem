using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.NutritionPlanTemplates;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.Foods;

/// <summary>Coach-facing CRUD over the food library (tenant-scoped).</summary>
[Authorize(CoachAppPermissions.Coach.Foods.Default)]
public class FoodAppService : CoachAppAppService, IFoodAppService
{
    private readonly IRepository<Food, Guid> _foodRepository;
    private readonly IRepository<NutritionPlan, Guid> _nutritionPlanRepository;
    private readonly IRepository<NutritionPlanTemplate, Guid> _nutritionPlanTemplateRepository;
    private readonly IRepository<NutritionLog, Guid> _nutritionLogRepository;

    public FoodAppService(
        IRepository<Food, Guid> foodRepository,
        IRepository<NutritionPlan, Guid> nutritionPlanRepository,
        IRepository<NutritionPlanTemplate, Guid> nutritionPlanTemplateRepository,
        IRepository<NutritionLog, Guid> nutritionLogRepository)
    {
        _foodRepository = foodRepository;
        _nutritionPlanRepository = nutritionPlanRepository;
        _nutritionPlanTemplateRepository = nutritionPlanTemplateRepository;
        _nutritionLogRepository = nutritionLogRepository;
    }

    public virtual async Task<FoodDto> GetAsync(Guid id)
    {
        var food = await _foodRepository.GetAsync(id);
        return ObjectMapper.Map<Food, FoodDto>(food);
    }

    public virtual async Task<PagedResultDto<FoodDto>> GetListAsync(GetFoodListInput input)
    {
        var query = await _foodRepository.GetQueryableAsync();

        query = query
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!)
                     || (x.Description != null && x.Description.Contains(input.Filter!)))
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(Food.Name)} asc" : input.Sorting;

        var foods = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<FoodDto>(
            totalCount,
            ObjectMapper.Map<List<Food>, List<FoodDto>>(foods));
    }

    [Authorize(CoachAppPermissions.Coach.Foods.Create)]
    public virtual async Task<FoodDto> CreateAsync(CreateUpdateFoodDto input)
    {
        var food = Food.Create(
            GuidGenerator.Create(),
            input.Name,
            input.ServingSize,
            input.ServingUnit,
            input.Calories,
            input.ProteinG,
            input.CarbsG,
            input.FatG,
            input.Description,
            CurrentTenant.Id);

        food.IsActive = input.IsActive;

        await _foodRepository.InsertAsync(food, autoSave: true);

        return ObjectMapper.Map<Food, FoodDto>(food);
    }

    [Authorize(CoachAppPermissions.Coach.Foods.Update)]
    public virtual async Task<FoodDto> UpdateAsync(Guid id, CreateUpdateFoodDto input)
    {
        var food = await _foodRepository.GetAsync(id);

        food.Name = input.Name;
        food.Description = input.Description;
        food.ServingSize = input.ServingSize;
        food.ServingUnit = input.ServingUnit;
        food.Calories = input.Calories;
        food.ProteinG = input.ProteinG;
        food.CarbsG = input.CarbsG;
        food.FatG = input.FatG;
        food.IsActive = input.IsActive;

        await _foodRepository.UpdateAsync(food, autoSave: true);

        return ObjectMapper.Map<Food, FoodDto>(food);
    }

    [Authorize(CoachAppPermissions.Coach.Foods.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        // Don't orphan references: block the delete if the food is used anywhere.
        // The check runs through the tenant-scoped aggregate roots (NutritionPlan,
        // NutritionPlanTemplate, NutritionLog), whose queryables are automatically filtered
        // to the current tenant — so it only ever sees this tenant's references and cannot
        // count or probe references belonging to another tenant.
        var planQuery = await _nutritionPlanRepository.GetQueryableAsync();
        var templateQuery = await _nutritionPlanTemplateRepository.GetQueryableAsync();
        var logQuery = await _nutritionLogRepository.GetQueryableAsync();

        if (await AsyncExecuter.AnyAsync(planQuery.Where(p => p.Meals.Any(m => m.Items.Any(i => i.FoodId == id))))
            || await AsyncExecuter.AnyAsync(templateQuery.Where(t => t.Meals.Any(m => m.Items.Any(i => i.FoodId == id))))
            || await AsyncExecuter.AnyAsync(logQuery.Where(l => l.Entries.Any(e => e.FoodId == id))))
        {
            throw new BusinessException(CoachAppDomainErrorCodes.FoodInUse);
        }

        await _foodRepository.DeleteAsync(id);
    }
}
