using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// One entry when editing a logged session. Actual Sets/Reps/WeightKg are what the trainee did.
/// The prescribed plan snapshot is server-owned and is NOT accepted from the client here — it is
/// preserved across the clear-and-rebuild update by the app service.
/// </summary>
public class UpdateWorkoutLogEntryDto
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
