using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.ProgressEntries;

/// <summary>
/// A dated snapshot of a trainee's body metrics (weight, body-fat, circumferences),
/// used to track progress over time. Recorded by the coach at check-ins or by the
/// trainee themselves. Tenant-scoped aggregate root.
/// </summary>
public class ProgressEntry : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual Guid TraineeId { get; set; }

    public virtual DateTime Date { get; set; }

    public virtual decimal? WeightKg { get; set; }

    public virtual decimal? BodyFatPercent { get; set; }

    public virtual decimal? ChestCm { get; set; }

    public virtual decimal? WaistCm { get; set; }

    public virtual decimal? HipsCm { get; set; }

    public virtual decimal? ArmCm { get; set; }

    public virtual decimal? ThighCm { get; set; }

    public virtual string? Notes { get; set; }

    /// <summary>For the ORM only.</summary>
    protected ProgressEntry()
    {
    }

    public static ProgressEntry Create(
        Guid id,
        Guid traineeId,
        DateTime date,
        decimal? weightKg = null,
        decimal? bodyFatPercent = null,
        decimal? chestCm = null,
        decimal? waistCm = null,
        decimal? hipsCm = null,
        decimal? armCm = null,
        decimal? thighCm = null,
        string? notes = null,
        Guid? tenantId = null)
    {
        return new ProgressEntry
        {
            Id = id,
            TraineeId = traineeId,
            Date = date,
            WeightKg = weightKg,
            BodyFatPercent = bodyFatPercent,
            ChestCm = chestCm,
            WaistCm = waistCm,
            HipsCm = hipsCm,
            ArmCm = armCm,
            ThighCm = thighCm,
            Notes = Check.Length(notes, nameof(notes), ProgressEntryConsts.MaxNotesLength),
            TenantId = tenantId
        };
    }
}
