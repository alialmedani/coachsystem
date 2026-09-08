using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.NutritionPlans;

namespace CoachApp.Entites.NutritionPlanTemplates;

public class CreateUpdateNutritionTemplateMealDto
{
    [Required]
    [StringLength(NutritionPlanConsts.MaxMealNameLength)]
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<CreateUpdateNutritionTemplateItemDto> Items { get; set; } = new();
}
