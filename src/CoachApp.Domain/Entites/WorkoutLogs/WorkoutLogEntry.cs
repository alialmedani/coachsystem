using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// One exercise the trainee actually performed in a logged session: the exercise
/// and the sets/reps/weight they did. Child entity of the WorkoutLog aggregate.
/// </summary>
public class WorkoutLogEntry : Entity<Guid>
{
    public virtual Guid WorkoutLogId { get; protected set; }

    public virtual Guid ExerciseId { get; set; }

    public virtual int Order { get; set; }

    public virtual int Sets { get; set; }

    public virtual string? Reps { get; set; }

    public virtual decimal? WeightKg { get; set; }

    public virtual string? Notes { get; set; }

    /// <summary>What the plan prescribed for this exercise, snapshotted when the session is logged
    /// from a plan. Kept separate from the actual Sets/Reps/WeightKg above so the historical record
    /// survives later edits to the plan, and planned-vs-done can be compared. Null for manual logs.</summary>
    public virtual int? PrescribedSets { get; set; }

    public virtual string? PrescribedReps { get; set; }

    public virtual decimal? PrescribedWeightKg { get; set; }

    protected WorkoutLogEntry()
    {
    }

    public WorkoutLogEntry(
        Guid id,
        Guid workoutLogId,
        Guid exerciseId,
        int order,
        int sets,
        string? reps = null,
        decimal? weightKg = null,
        string? notes = null,
        int? prescribedSets = null,
        string? prescribedReps = null,
        decimal? prescribedWeightKg = null)
        : base(id)
    {
        WorkoutLogId = workoutLogId;
        ExerciseId = exerciseId;
        Order = order;
        Sets = sets;
        Reps = Check.Length(reps, nameof(reps), WorkoutLogConsts.MaxRepsLength);
        WeightKg = weightKg;
        Notes = Check.Length(notes, nameof(notes), WorkoutLogConsts.MaxNotesLength);
        PrescribedSets = prescribedSets;
        PrescribedReps = Check.Length(prescribedReps, nameof(prescribedReps), WorkoutLogConsts.MaxRepsLength);
        PrescribedWeightKg = prescribedWeightKg;
    }
}
