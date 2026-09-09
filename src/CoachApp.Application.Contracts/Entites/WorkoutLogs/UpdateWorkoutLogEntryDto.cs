using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// One entry when editing a logged session. Actual Sets/Reps/WeightKg are what the trainee
/// did; the Prescribed* fields carry the plan snapshot back so it is preserved across the
/// clear-and-rebuild update (informational only — the server does not authorize on them).
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

    public int? PrescribedSets { get; set; }

    [StringLength(WorkoutLogConsts.MaxRepsLength)]
    public string? PrescribedReps { get; set; }

    [Range(0, 1000)]
    public decimal? PrescribedWeightKg { get; set; }
}
