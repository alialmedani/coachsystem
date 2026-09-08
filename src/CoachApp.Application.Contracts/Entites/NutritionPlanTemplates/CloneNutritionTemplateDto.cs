using System;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.NutritionPlans;

namespace CoachApp.Entites.NutritionPlanTemplates;

/// <summary>
/// Clones a template onto a trainee, creating a new (inactive) <c>NutritionPlan</c>. Optional
/// <see cref="Name"/>/<see cref="Description"/> override the template's; otherwise the template's are used.
/// </summary>
public class CloneNutritionTemplateDto
{
    [Required]
    public Guid TraineeId { get; set; }

    [StringLength(NutritionPlanConsts.MaxNameLength)]
    public string? Name { get; set; }

    [StringLength(NutritionPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
}
