using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.TrainingPlans;

/// <summary>
/// TrainingPlan aggregate root. Pragmatic domain model: most properties are
/// publicly settable, but the date range is guarded by <see cref="SetSchedule"/>
/// (its one real invariant: a plan must end after it starts). The plan references
/// its <c>Trainee</c> by id only — they are separate aggregate roots, so there is
/// no navigation across the boundary; trainee existence is checked in the
/// application service.
/// </summary>
public class TrainingPlan : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual Guid TraineeId { get; set; }

    public virtual string Title { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual DateTime StartDate { get; protected set; }

    public virtual DateTime EndDate { get; protected set; }

    public virtual bool IsActive { get; set; }

    /// <summary>For the ORM only.</summary>
    protected TrainingPlan()
    {
    }

    /// <summary>
    /// Creates a valid plan: generates identity at the call site, validates the
    /// required fields, enforces the date-range invariant and stamps the tenant.
    /// </summary>
    public static TrainingPlan Create(
        Guid id,
        Guid traineeId,
        string title,
        DateTime startDate,
        DateTime endDate,
        string? description = null,
        Guid? tenantId = null)
    {
        var plan = new TrainingPlan
        {
            Id = id,
            TraineeId = traineeId,
            Title = Check.NotNullOrWhiteSpace(title, nameof(title), TrainingPlanConsts.MaxTitleLength),
            Description = Check.Length(description, nameof(description), TrainingPlanConsts.MaxDescriptionLength),
            TenantId = tenantId,
            IsActive = true
        };

        plan.SetSchedule(startDate, endDate);
        return plan;
    }

    /// <summary>
    /// Sets the plan's date range, enforcing the invariant that it must end after
    /// it starts.
    /// </summary>
    public virtual void SetSchedule(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.TrainingPlanInvalidDateRange)
                .WithData("StartDate", startDate)
                .WithData("EndDate", endDate);
        }

        StartDate = startDate;
        EndDate = endDate;
    }

    public virtual void Activate() => IsActive = true;

    public virtual void Deactivate() => IsActive = false;
}
