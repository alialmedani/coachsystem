using System;
using System.Collections.Generic;
using CoachApp.Entites.WorkoutPlans;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.WorkoutPlanTemplates;

/// <summary>
/// A reusable workout plan blueprint (not tied to any trainee), composed of ordered days
/// each with prescribed exercises. Coaches build a library of these and clone them onto
/// trainees (producing a normal <see cref="WorkoutPlan"/>), or snapshot an existing plan
/// into one. Tenant-scoped aggregate root; days/exercises are managed through it. Mirrors
/// <see cref="WorkoutPlan"/> but without <c>TraineeId</c> or <c>IsActive</c>.
/// </summary>
public class WorkoutPlanTemplate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual ICollection<WorkoutTemplateDay> Days { get; protected set; }

    protected WorkoutPlanTemplate()
    {
        Days = new List<WorkoutTemplateDay>();
    }

    public static WorkoutPlanTemplate Create(
        Guid id,
        string name,
        string? description = null,
        Guid? tenantId = null)
    {
        return new WorkoutPlanTemplate
        {
            Id = id,
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), WorkoutPlanConsts.MaxNameLength),
            Description = Check.Length(description, nameof(description), WorkoutPlanConsts.MaxDescriptionLength),
            TenantId = tenantId,
            Days = new List<WorkoutTemplateDay>()
        };
    }

    public WorkoutTemplateDay AddDay(Guid id, string name, int order, DayOfWeek? scheduledDay = null)
    {
        var day = new WorkoutTemplateDay(id, Id, name, order, scheduledDay);
        Days.Add(day);
        return day;
    }

    /// <summary>Removes all days (and their exercises) — used for a full edit/replace.</summary>
    public void ClearDays() => Days.Clear();
}
