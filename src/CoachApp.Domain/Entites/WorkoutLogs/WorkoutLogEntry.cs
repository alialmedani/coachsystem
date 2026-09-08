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
        string? notes = null)
        : base(id)
    {
        WorkoutLogId = workoutLogId;
        ExerciseId = exerciseId;
        Order = order;
        Sets = sets;
        Reps = Check.Length(reps, nameof(reps), WorkoutLogConsts.MaxRepsLength);
        WeightKg = weightKg;
        Notes = Check.Length(notes, nameof(notes), WorkoutLogConsts.MaxNotesLength);
    }
}
