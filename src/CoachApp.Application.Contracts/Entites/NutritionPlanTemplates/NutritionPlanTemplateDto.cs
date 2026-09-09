using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionPlanTemplates;

public class NutritionPlanTemplateDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Optional coach-set daily targets (null when not set); carried to the cloned plan.
    public decimal? TargetCalories { get; set; }
    public decimal? TargetProteinG { get; set; }
    public decimal? TargetCarbsG { get; set; }
    public decimal? TargetFatG { get; set; }

    public List<NutritionTemplateMealDto> Meals { get; set; } = new();

    // Plan totals, summed from all meal items (enriched in the app service).
    public decimal TotalCalories { get; set; }
    public decimal TotalProteinG { get; set; }
    public decimal TotalCarbsG { get; set; }
    public decimal TotalFatG { get; set; }
}
