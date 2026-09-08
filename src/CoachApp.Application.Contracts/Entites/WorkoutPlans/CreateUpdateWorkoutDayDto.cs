using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutPlans;

public class CreateUpdateWorkoutDayDto
{
    [Required]
    [StringLength(WorkoutPlanConsts.MaxDayNameLength)]
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<CreateUpdateWorkoutExerciseDto> Exercises { get; set; } = new();
}
