using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.WorkoutPlans;

/// <summary>
/// A workout plan the coach assigns to a trainee, composed of ordered days, each
/// with prescribed exercises. Aggregate root; the days/exercises are managed through
/// it. A trainee may have several plans but at most one <see cref="IsActive"/> at a
/// time (enforced by the application service).
/// </summary>
public class WorkoutPlan : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual Guid TraineeId { get; set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual bool IsActive { get; set; }

    public virtual ICollection<WorkoutDay> Days { get; protected set; }

    protected WorkoutPlan()
    {
        Days = new List<WorkoutDay>();
    }

    public static WorkoutPlan Create(
        Guid id,
        Guid traineeId,
        string name,
        string? description = null,
        Guid? tenantId = null)
    {
        return new WorkoutPlan
        {
            Id = id,
            TraineeId = traineeId,
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), WorkoutPlanConsts.MaxNameLength),
            Description = Check.Length(description, nameof(description), WorkoutPlanConsts.MaxDescriptionLength),
            TenantId = tenantId,
            IsActive = false,
            Days = new List<WorkoutDay>()
        };
    }

    public WorkoutDay AddDay(Guid id, string name, int order, DayOfWeek? scheduledDay = null)
    {
        var day = new WorkoutDay(id, Id, name, order, scheduledDay);
        Days.Add(day);
        return day;
    }

    /// <summary>Removes all days (and their exercises) — used for a full edit/replace.</summary>
    public void ClearDays() => Days.Clear();

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
