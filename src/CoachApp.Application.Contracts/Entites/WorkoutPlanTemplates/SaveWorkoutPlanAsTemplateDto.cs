using System;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.WorkoutPlans;

namespace CoachApp.Entites.WorkoutPlanTemplates;

/// <summary>Snapshots an existing trainee <c>WorkoutPlan</c> into a new reusable template.</summary>
public class SaveWorkoutPlanAsTemplateDto
{
    [Required]
    public Guid WorkoutPlanId { get; set; }

    [Required]
    [StringLength(WorkoutPlanConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(WorkoutPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
}
