using System;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.NutritionPlans;

namespace CoachApp.Entites.NutritionPlanTemplates;

/// <summary>Snapshots an existing trainee <c>NutritionPlan</c> into a new reusable template.</summary>
public class SaveNutritionPlanAsTemplateDto
{
    [Required]
    public Guid NutritionPlanId { get; set; }

    [Required]
    [StringLength(NutritionPlanConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(NutritionPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
}
