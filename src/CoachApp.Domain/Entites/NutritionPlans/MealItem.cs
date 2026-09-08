using System;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.NutritionPlans;

/// <summary>
/// One food in a <see cref="Meal"/>, with the quantity (number of the food's
/// servings). Calories/macros are computed from the food library at read time.
/// Child entity of the NutritionPlan aggregate.
/// </summary>
public class MealItem : Entity<Guid>
{
    public virtual Guid MealId { get; protected set; }

    public virtual Guid FoodId { get; set; }

    public virtual int Order { get; set; }

    /// <summary>Number of the food's servings in this meal (e.g. 1.5).</summary>
    public virtual decimal Quantity { get; set; }

    protected MealItem()
    {
    }

    public MealItem(Guid id, Guid mealId, Guid foodId, int order, decimal quantity)
        : base(id)
    {
        MealId = mealId;
        FoodId = foodId;
        Order = order;
        Quantity = quantity;
    }
}
