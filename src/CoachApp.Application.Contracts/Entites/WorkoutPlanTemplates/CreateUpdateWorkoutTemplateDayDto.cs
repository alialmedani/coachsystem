using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoachApp.Entites.WorkoutPlans;

namespace CoachApp.Entites.WorkoutPlanTemplates;

public class CreateUpdateWorkoutTemplateDayDto
{
    [Required]
    [StringLength(WorkoutPlanConsts.MaxDayNameLength)]
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<CreateUpdateWorkoutTemplateExerciseDto> Exercises { get; set; } = new();
}
