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

    public List<CreateUpdateNutritionTemplateMealDto> Meals { get; set; } = new();
}
