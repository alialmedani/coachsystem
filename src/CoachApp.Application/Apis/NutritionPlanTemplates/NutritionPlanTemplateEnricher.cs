using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionPlanTemplates;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.NutritionPlanTemplates;

/// <summary>
/// Fills each <see cref="NutritionTemplateItemDto"/> with the food's name/unit and computed
/// calories/macros (per-serving values * quantity), and sums the template totals. Mirrors
/// <c>NutritionPlanEnricher</c> for the template DTO shape.
/// </summary>
internal static class NutritionPlanTemplateEnricher
{
    public static async Task EnrichAsync(NutritionPlanTemplateDto dto, IRepository<Food, Guid> foodRepository)
    {
        var ids = dto.Meals.SelectMany(m => m.Items).Select(i => i.FoodId).Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var foods = (await foodRepository.GetListAsync(f => ids.Contains(f.Id))).ToDictionary(f => f.Id);

        decimal totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0;

        foreach (var meal in dto.Meals)
        {
            foreach (var item in meal.Items)
            {
                if (!foods.TryGetValue(item.FoodId, out var food))
                {
                    continue;
                }

                item.FoodName = food.Name;
                item.ServingUnit = food.ServingUnit;
                item.Calories = food.Calories * item.Quantity;
                item.ProteinG = food.ProteinG * item.Quantity;
                item.CarbsG = food.CarbsG * item.Quantity;
                item.FatG = food.FatG * item.Quantity;

                totalCalories += item.Calories;
                totalProtein += item.ProteinG;
                totalCarbs += item.CarbsG;
                totalFat += item.FatG;
            }
        }

        dto.TotalCalories = totalCalories;
        dto.TotalProteinG = totalProtein;
        dto.TotalCarbsG = totalCarbs;
        dto.TotalFatG = totalFat;
    }
}
