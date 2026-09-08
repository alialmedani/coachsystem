using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionLogs;

public class NutritionLogEntryDto : EntityDto<Guid>
{
    public Guid FoodId { get; set; }

    public string? FoodName { get; set; }

    public string? ServingUnit { get; set; }

    public decimal Quantity { get; set; }

    public int Order { get; set; }

    public string? Notes { get; set; }

    // Computed from the food's per-serving values * Quantity (enriched in the app service).
    public decimal Calories { get; set; }
    public decimal ProteinG { get; set; }
    public decimal CarbsG { get; set; }
    public decimal FatG { get; set; }
}
