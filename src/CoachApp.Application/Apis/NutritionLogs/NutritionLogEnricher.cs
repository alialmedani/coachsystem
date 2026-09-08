using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.NutritionLogs;

/// <summary>Fills each entry with the food's name/unit and computed macros, and sums totals.</summary>
internal static class NutritionLogEnricher
{
    public static async Task EnrichAsync(NutritionLogDto dto, IRepository<Food, Guid> foodRepository)
    {
        var ids = dto.Entries.Select(e => e.FoodId).Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var foods = (await foodRepository.GetListAsync(f => ids.Contains(f.Id))).ToDictionary(f => f.Id);

        decimal totalCalories = 0, totalProtein = 0, totalCarbs = 0, totalFat = 0;

        foreach (var entry in dto.Entries)
        {
            if (!foods.TryGetValue(entry.FoodId, out var food))
            {
                continue;
            }

            entry.FoodName = food.Name;
            entry.ServingUnit = food.ServingUnit;
            entry.Calories = food.Calories * entry.Quantity;
            entry.ProteinG = food.ProteinG * entry.Quantity;
            entry.CarbsG = food.CarbsG * entry.Quantity;
            entry.FatG = food.FatG * entry.Quantity;

            totalCalories += entry.Calories;
            totalProtein += entry.ProteinG;
            totalCarbs += entry.CarbsG;
            totalFat += entry.FatG;
        }

        dto.TotalCalories = totalCalories;
        dto.TotalProteinG = totalProtein;
        dto.TotalCarbsG = totalCarbs;
        dto.TotalFatG = totalFat;
    }
}
