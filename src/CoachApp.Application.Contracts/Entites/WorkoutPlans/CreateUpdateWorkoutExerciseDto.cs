using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutPlans;

public class CreateUpdateWorkoutExerciseDto
{
    [Required]
    public Guid ExerciseId { get; set; }

    public int Order { get; set; }

    [Range(1, 100)]
    public int Sets { get; set; } = 1;

    [StringLength(WorkoutPlanConsts.MaxRepsLength)]
    public string? Reps { get; set; }

    [Range(0, 1000)]
    public decimal? WeightKg { get; set; }

    [Range(0, 3600)]
    public int? RestSeconds { get; set; }

    [StringLength(WorkoutPlanConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
