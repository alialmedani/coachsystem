using System;
using CoachApp.Entites.WorkoutPlans;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.WorkoutPlanTemplates;

/// <summary>
/// One prescribed exercise within a <see cref="WorkoutTemplateDay"/>: which library exercise,
/// and the target sets / reps / weight / rest. Child entity of the template aggregate.
/// </summary>
public class WorkoutTemplateExercise : Entity<Guid>
{
    public virtual Guid WorkoutTemplateDayId { get; protected set; }

    /// <summary>The library <c>Exercise</c> being prescribed.</summary>
    public virtual Guid ExerciseId { get; set; }

    public virtual int Order { get; set; }

    public virtual int Sets { get; set; }

    /// <summary>Free-form rep target, e.g. "8-12" or "AMRAP".</summary>
    public virtual string? Reps { get; set; }

    public virtual decimal? WeightKg { get; set; }

    public virtual int? RestSeconds { get; set; }

    public virtual string? Notes { get; set; }

    protected WorkoutTemplateExercise()
    {
    }

    public WorkoutTemplateExercise(
        Guid id,
        Guid workoutTemplateDayId,
        Guid exerciseId,
        int order,
        int sets,
        string? reps = null,
        decimal? weightKg = null,
        int? restSeconds = null,
        string? notes = null)
        : base(id)
    {
        WorkoutTemplateDayId = workoutTemplateDayId;
        ExerciseId = exerciseId;
        Order = order;
        Sets = sets;
        Reps = Check.Length(reps, nameof(reps), WorkoutPlanConsts.MaxRepsLength);
        WeightKg = weightKg;
        RestSeconds = restSeconds;
        Notes = Check.Length(notes, nameof(notes), WorkoutPlanConsts.MaxNotesLength);
    }
}
