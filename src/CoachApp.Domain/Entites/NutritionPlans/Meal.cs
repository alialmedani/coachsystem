using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.NutritionPlans;

/// <summary>
/// A meal within a <see cref="NutritionPlan"/> (e.g. "Breakfast"), owning its
/// ordered food items. Child entity of the NutritionPlan aggregate.
/// </summary>
public class Meal : Entity<Guid>
{
    public virtual Guid NutritionPlanId { get; protected set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual int Order { get; set; }

    public virtual ICollection<MealItem> Items { get; protected set; }

    protected Meal()
    {
        Items = new List<MealItem>();
    }

    public Meal(Guid id, Guid nutritionPlanId, string name, int order)
        : base(id)
    {
        NutritionPlanId = nutritionPlanId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), NutritionPlanConsts.MaxMealNameLength);
        Order = order;
        Items = new List<MealItem>();
    }

    public MealItem AddItem(Guid id, Guid foodId, int order, decimal quantity)
    {
        var item = new MealItem(id, Id, foodId, order, quantity);
        Items.Add(item);
        return item;
    }
}
