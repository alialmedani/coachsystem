using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// A logged workout session: when the trainee trained, optionally which plan/day,
/// and the exercises they actually performed. Created by the trainee (self-logging);
/// coaches read it to track adherence and progress. Aggregate root.
/// </summary>
public class WorkoutLog : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual Guid TraineeId { get; set; }

    /// <summary>Optional link to the plan the session followed.</summary>
    public virtual Guid? WorkoutPlanId { get; set; }

    /// <summary>Optional link to the specific day of the plan.</summary>
    public virtual Guid? WorkoutDayId { get; set; }

    public virtual DateTime Date { get; set; }

    public virtual string? Notes { get; set; }

    public virtual ICollection<WorkoutLogEntry> Entries { get; protected set; }

    protected WorkoutLog()
    {
        Entries = new List<WorkoutLogEntry>();
    }

    public static WorkoutLog Create(
        Guid id,
        Guid traineeId,
        DateTime date,
        Guid? workoutPlanId = null,
        Guid? workoutDayId = null,
        string? notes = null,
        Guid? tenantId = null)
    {
        return new WorkoutLog
        {
            Id = id,
            TraineeId = traineeId,
            Date = date,
            WorkoutPlanId = workoutPlanId,
            WorkoutDayId = workoutDayId,
            Notes = Check.Length(notes, nameof(notes), WorkoutLogConsts.MaxNotesLength),
            TenantId = tenantId,
            Entries = new List<WorkoutLogEntry>()
        };
    }

    public WorkoutLogEntry AddEntry(
        Guid id,
        Guid exerciseId,
        int order,
        int sets,
        string? reps = null,
        decimal? weightKg = null,
        string? notes = null)
    {
        var entry = new WorkoutLogEntry(id, Id, exerciseId, order, sets, reps, weightKg, notes);
        Entries.Add(entry);
        return entry;
    }
}
