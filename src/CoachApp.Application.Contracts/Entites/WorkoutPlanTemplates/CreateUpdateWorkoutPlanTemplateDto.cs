using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.WorkoutPlans;

namespace CoachApp.Entites.WorkoutPlanTemplates;

public class CreateUpdateWorkoutPlanTemplateDto
{
    [Required]
    [StringLength(WorkoutPlanConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(WorkoutPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public List<CreateUpdateWorkoutTemplateDayDto> Days { get; set; } = new();
}
