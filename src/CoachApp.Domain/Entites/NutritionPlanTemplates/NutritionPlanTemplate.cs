using System;
using System.Collections.Generic;
using CoachApp.Entites.NutritionPlans;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.NutritionPlanTemplates;

/// <summary>
/// A reusable nutrition plan blueprint (not tied to any trainee), composed of ordered meals
/// each with foods and quantities. Coaches build a library of these and clone them onto trainees
/// (producing a normal <see cref="NutritionPlan"/>), or snapshot an existing plan into one.
/// Tenant-scoped aggregate root; meals/items are managed through it. Mirrors
/// <see cref="NutritionPlan"/> but without <c>TraineeId</c> or <c>IsActive</c>.
/// </summary>
public class NutritionPlanTemplate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual string? Description { get; set; }

    public virtual ICollection<NutritionTemplateMeal> Meals { get; protected set; }

    protected NutritionPlanTemplate()
    {
        Meals = new List<NutritionTemplateMeal>();
    }

    public static NutritionPlanTemplate Create(
        Guid id,
        string name,
        string? description = null,
        Guid? tenantId = null)
    {
        return new NutritionPlanTemplate
        {
            Id = id,
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), NutritionPlanConsts.MaxNameLength),
            Description = Check.Length(description, nameof(description), NutritionPlanConsts.MaxDescriptionLength),
            TenantId = tenantId,
            Meals = new List<NutritionTemplateMeal>()
        };
    }

    public NutritionTemplateMeal AddMeal(Guid id, string name, int order)
    {
        var meal = new NutritionTemplateMeal(id, Id, name, order);
        Meals.Add(meal);
        return meal;
    }

    /// <summary>Removes all meals (and their items) — used for a full edit/replace.</summary>
    public void ClearMeals() => Meals.Clear();
}
