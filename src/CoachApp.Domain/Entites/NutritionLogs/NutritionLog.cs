using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>
/// A logged day of eating: when, optionally which nutrition plan it followed, and
/// the foods the trainee actually consumed. Created by the trainee (self-logging);
/// coaches read it to track nutrition adherence. Aggregate root.
/// </summary>
public class NutritionLog : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual Guid TraineeId { get; set; }

    public virtual Guid? NutritionPlanId { get; set; }

    public virtual DateTime Date { get; set; }

    public virtual string? Notes { get; set; }

    public virtual ICollection<NutritionLogEntry> Entries { get; protected set; }

    protected NutritionLog()
    {
        Entries = new List<NutritionLogEntry>();
    }

    public static NutritionLog Create(
        Guid id,
        Guid traineeId,
        DateTime date,
        Guid? nutritionPlanId = null,
        string? notes = null,
        Guid? tenantId = null)
    {
        return new NutritionLog
        {
            Id = id,
            TraineeId = traineeId,
            Date = date,
            NutritionPlanId = nutritionPlanId,
            Notes = Check.Length(notes, nameof(notes), NutritionLogConsts.MaxNotesLength),
            TenantId = tenantId,
            Entries = new List<NutritionLogEntry>()
        };
    }

    public NutritionLogEntry AddEntry(Guid id, Guid foodId, int order, decimal quantity, string? notes = null)
    {
        var entry = new NutritionLogEntry(id, Id, foodId, order, quantity, notes);
        Entries.Add(entry);
        return entry;
    }
}
