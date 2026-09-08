using System;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.NutritionPlanTemplates;

/// <summary>
/// One food in a <see cref="NutritionTemplateMeal"/>, with the quantity (number of the food's
/// servings). Calories/macros are computed from the food library at read time. Child entity
/// of the template aggregate.
/// </summary>
public class NutritionTemplateItem : Entity<Guid>
{
    public virtual Guid NutritionTemplateMealId { get; protected set; }

    public virtual Guid FoodId { get; set; }

    public virtual int Order { get; set; }

    /// <summary>Number of the food's servings in this meal (e.g. 1.5).</summary>
    public virtual decimal Quantity { get; set; }

    protected NutritionTemplateItem()
    {
    }

    public NutritionTemplateItem(Guid id, Guid nutritionTemplateMealId, Guid foodId, int order, decimal quantity)
        : base(id)
    {
        NutritionTemplateMealId = nutritionTemplateMealId;
        FoodId = foodId;
        Order = order;
        Quantity = quantity;
    }
}
