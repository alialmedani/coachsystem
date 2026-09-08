using System;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.WorkoutPlans;

namespace CoachApp.Entites.WorkoutPlanTemplates;

/// <summary>
/// Clones a template onto a trainee, creating a new (inactive) <c>WorkoutPlan</c>. Optional
/// <see cref="Name"/>/<see cref="Description"/> override the template's; otherwise the template's are used.
/// </summary>
public class CloneWorkoutTemplateDto
{
    [Required]
    public Guid TraineeId { get; set; }

    [StringLength(WorkoutPlanConsts.MaxNameLength)]
    public string? Name { get; set; }

    [StringLength(WorkoutPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }
}
