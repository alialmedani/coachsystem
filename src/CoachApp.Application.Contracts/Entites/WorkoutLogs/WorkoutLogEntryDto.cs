using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutLogs;

public class WorkoutLogEntryDto : EntityDto<Guid>
{
    public Guid ExerciseId { get; set; }

    /// <summary>Enriched from the exercise library for display.</summary>
    public string? ExerciseName { get; set; }

    public int Order { get; set; }

    public int Sets { get; set; }

    public string? Reps { get; set; }

    public decimal? WeightKg { get; set; }

    public string? Notes { get; set; }

    /// <summary>What the plan prescribed for this exercise (snapshotted when logged from a plan);
    /// null for manually created logs.</summary>
    public int? PrescribedSets { get; set; }

    public string? PrescribedReps { get; set; }

    public decimal? PrescribedWeightKg { get; set; }
}
