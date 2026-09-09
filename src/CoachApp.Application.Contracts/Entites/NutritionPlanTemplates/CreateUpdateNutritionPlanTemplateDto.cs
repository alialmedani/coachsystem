using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.NutritionPlans;

namespace CoachApp.Entites.NutritionPlanTemplates;

public class CreateUpdateNutritionPlanTemplateDto
{
    [Required]
    [StringLength(NutritionPlanConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(NutritionPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    // Optional coach-set daily targets; leave null to use the plan's meal totals.
    [Range(0, 100000)] public decimal? TargetCalories { get; set; }
    [Range(0, 100000)] public decimal? TargetProteinG { get; set; }
    [Range(0, 100000)] public decimal? TargetCarbsG { get; set; }
    [Range(0, 100000)] public decimal? TargetFatG { get; set; }

    public List<CreateUpdateNutritionTemplateMealDto> Meals { get; set; } = new();
}
