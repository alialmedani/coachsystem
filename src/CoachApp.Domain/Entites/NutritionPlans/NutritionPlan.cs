using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.NutritionPlans;

/// <summary>
/// A nutrition plan the coach assigns to a trainee, composed of ordered meals, each
/// with foods and quantities. Aggregate root; meals/items are managed through it.
/// A trainee may have several plans but at most one <see cref="IsActive"/> at a time
/// (enforced by the application service).
/// </summary>
public class NutritionPlan : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual Guid TraineeId { get; set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual bool IsActive { get; set; }

    public virtual ICollection<Meal> Meals { get; protected set; }

    protected NutritionPlan()
    {
        Meals = new List<Meal>();
    }

    public static NutritionPlan Create(
        Guid id,
        Guid traineeId,
        string name,
        string? description = null,
        Guid? tenantId = null)
    {
        return new NutritionPlan
        {
            Id = id,
            TraineeId = traineeId,
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), NutritionPlanConsts.MaxNameLength),
            Description = Check.Length(description, nameof(description), NutritionPlanConsts.MaxDescriptionLength),
            TenantId = tenantId,
            IsActive = false,
            Meals = new List<Meal>()
        };
    }

    public Meal AddMeal(Guid id, string name, int order)
    {
        var meal = new Meal(id, Id, name, order);
        Meals.Add(meal);
        return meal;
    }

    /// <summary>Removes all meals (and their items) — used for a full edit/replace.</summary>
    public void ClearMeals() => Meals.Clear();

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
