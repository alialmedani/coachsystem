using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionPlans;

public class NutritionPlanDto : FullAuditedEntityDto<Guid>
{
    public Guid TraineeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public List<MealDto> Meals { get; set; } = new();

    // Plan totals, summed from all meal items (enriched in the app service).
    public decimal TotalCalories { get; set; }
    public decimal TotalProteinG { get; set; }
    public decimal TotalCarbsG { get; set; }
    public decimal TotalFatG { get; set; }
}
