using System;
using System.Collections.Generic;
using CoachApp.Entites.WorkoutPlans;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.WorkoutPlanTemplates;

/// <summary>
/// A day/session within a <see cref="WorkoutPlanTemplate"/>, owning its ordered exercises.
/// Child entity of the template aggregate.
/// </summary>
public class WorkoutTemplateDay : Entity<Guid>
{
    public virtual Guid WorkoutPlanTemplateId { get; protected set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual int Order { get; set; }

    public virtual ICollection<WorkoutTemplateExercise> Exercises { get; protected set; }

    protected WorkoutTemplateDay()
    {
        Exercises = new List<WorkoutTemplateExercise>();
    }

    public WorkoutTemplateDay(Guid id, Guid workoutPlanTemplateId, string name, int order)
        : base(id)
    {
        WorkoutPlanTemplateId = workoutPlanTemplateId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), WorkoutPlanConsts.MaxDayNameLength);
        Order = order;
        Exercises = new List<WorkoutTemplateExercise>();
    }

    public WorkoutTemplateExercise AddExercise(
        Guid id,
        Guid exerciseId,
        int order,
        int sets,
        string? reps = null,
        decimal? weightKg = null,
        int? restSeconds = null,
        string? notes = null)
    {
        var exercise = new WorkoutTemplateExercise(id, Id, exerciseId, order, sets, reps, weightKg, restSeconds, notes);
        Exercises.Add(exercise);
        return exercise;
    }
}
