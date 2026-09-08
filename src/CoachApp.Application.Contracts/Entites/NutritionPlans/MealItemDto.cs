using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionPlans;

public class MealItemDto : EntityDto<Guid>
{
    public Guid FoodId { get; set; }

    /// <summary>Enriched from the food library for display.</summary>
    public string? FoodName { get; set; }

    public string? ServingUnit { get; set; }

    /// <summary>Number of the food's servings.</summary>
    public decimal Quantity { get; set; }

    public int Order { get; set; }

    // Computed from the food's per-serving values * Quantity (enriched in the app service).
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}
