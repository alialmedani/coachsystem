using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.TraineeNotes;

/// <summary>
/// A note the coach writes about (and for) a trainee. The author is the audit
/// <c>CreatorId</c>. Visible to the trainee. Tenant-scoped aggregate root.
/// </summary>
public class TraineeNote : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual Guid TraineeId { get; set; }

    public virtual DateTime Date { get; set; }

    public virtual string Text { get; set; } = string.Empty;

    /// <summary>For the ORM only.</summary>
    protected TraineeNote()
    {
    }

    public static TraineeNote Create(Guid id, Guid traineeId, DateTime date, string text, Guid? tenantId = null)
    {
        return new TraineeNote
        {
            Id = id,
            TraineeId = traineeId,
            Date = date,
            Text = Check.NotNullOrWhiteSpace(text, nameof(text), TraineeNoteConsts.MaxTextLength),
            TenantId = tenantId
        };
    }
}
