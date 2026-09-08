using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutLogs;

public class CreateWorkoutLogEntryDto
{
    [Required]
    public Guid ExerciseId { get; set; }

    public int Order { get; set; }

    [Range(0, 100)]
    public int Sets { get; set; }

    [StringLength(WorkoutLogConsts.MaxRepsLength)]
    public string? Reps { get; set; }

    [Range(0, 1000)]
    public decimal? WeightKg { get; set; }

    [StringLength(WorkoutLogConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
