using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.NutritionPlans;

public class CreateUpdateNutritionPlanDto
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    [StringLength(NutritionPlanConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(NutritionPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    // Optional coach-set daily targets; leave null to use the plan's meal totals.
    [Range(0, 100000)] public decimal? TargetCalories { get; set; }
    [Range(0, 100000)] public decimal? TargetProteinG { get; set; }
    [Range(0, 100000)] public decimal? TargetCarbsG { get; set; }
    [Range(0, 100000)] public decimal? TargetFatG { get; set; }

    public List<CreateUpdateMealDto> Meals { get; set; } = new();
}
