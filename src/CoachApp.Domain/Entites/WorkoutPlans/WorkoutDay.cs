using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.WorkoutPlans;

/// <summary>
/// A day/session within a <see cref="WorkoutPlan"/> (e.g. "Day 1 - Push"), owning
/// its ordered exercises. Child entity of the WorkoutPlan aggregate.
/// </summary>
public class WorkoutDay : Entity<Guid>
{
    public virtual Guid WorkoutPlanId { get; protected set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual int Order { get; set; }

    public virtual ICollection<WorkoutExercise> Exercises { get; protected set; }

    protected WorkoutDay()
    {
        Exercises = new List<WorkoutExercise>();
    }

    public WorkoutDay(Guid id, Guid workoutPlanId, string name, int order)
        : base(id)
    {
        WorkoutPlanId = workoutPlanId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), WorkoutPlanConsts.MaxDayNameLength);
        Order = order;
        Exercises = new List<WorkoutExercise>();
    }

    public WorkoutExercise AddExercise(
        Guid id,
        Guid exerciseId,
        int order,
        int sets,
        string? reps = null,
        decimal? weightKg = null,
        int? restSeconds = null,
        string? notes = null)
    {
        var exercise = new WorkoutExercise(id, Id, exerciseId, order, sets, reps, weightKg, restSeconds, notes);
        Exercises.Add(exercise);
        return exercise;
    }
}
