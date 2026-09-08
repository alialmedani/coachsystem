using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.Foods;

/// <summary>
/// A food in the coach's library (tenant-scoped). Nutritional values are expressed
/// per one serving (<see cref="ServingSize"/> <see cref="ServingUnit"/>). Referenced
/// by nutrition plans. Pragmatic domain: public setters + a validating <see cref="Create"/>.
/// </summary>
public class Food : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual decimal ServingSize { get; set; }

    public virtual string ServingUnit { get; set; } = "g";

    public virtual decimal Calories { get; set; }

    public virtual decimal ProteinG { get; set; }

    public virtual decimal CarbsG { get; set; }

    public virtual decimal FatG { get; set; }

    public virtual bool IsActive { get; set; }

    /// <summary>For the ORM only.</summary>
    protected Food()
    {
    }

    public static Food Create(
        Guid id,
        string name,
        decimal servingSize,
        string servingUnit,
        decimal calories,
        decimal proteinG,
        decimal carbsG,
        decimal fatG,
        string? description = null,
        Guid? tenantId = null)
    {
        return new Food
        {
            Id = id,
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), FoodConsts.MaxNameLength),
            ServingSize = servingSize,
            ServingUnit = Check.NotNullOrWhiteSpace(servingUnit, nameof(servingUnit), FoodConsts.MaxServingUnitLength),
            Calories = calories,
            ProteinG = proteinG,
            CarbsG = carbsG,
            FatG = fatG,
            Description = Check.Length(description, nameof(description), FoodConsts.MaxDescriptionLength),
            TenantId = tenantId,
            IsActive = true
        };
    }

    public virtual void Activate() => IsActive = true;

    public virtual void Deactivate() => IsActive = false;
}
