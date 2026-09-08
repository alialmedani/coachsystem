using System;
using System.Collections.Generic;
using CoachApp.Entites.NutritionPlans;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.NutritionPlanTemplates;

/// <summary>
/// A meal within a <see cref="NutritionPlanTemplate"/>, owning its ordered food items.
/// Child entity of the template aggregate.
/// </summary>
public class NutritionTemplateMeal : Entity<Guid>
{
    public virtual Guid NutritionPlanTemplateId { get; protected set; }

    public virtual string Name { get; set; } = string.Empty;

    public virtual int Order { get; set; }

    public virtual ICollection<NutritionTemplateItem> Items { get; protected set; }

    protected NutritionTemplateMeal()
    {
        Items = new List<NutritionTemplateItem>();
    }

    public NutritionTemplateMeal(Guid id, Guid nutritionPlanTemplateId, string name, int order)
        : base(id)
    {
        NutritionPlanTemplateId = nutritionPlanTemplateId;
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), NutritionPlanConsts.MaxMealNameLength);
        Order = order;
        Items = new List<NutritionTemplateItem>();
    }

    public NutritionTemplateItem AddItem(Guid id, Guid foodId, int order, decimal quantity)
    {
        var item = new NutritionTemplateItem(id, Id, foodId, order, quantity);
        Items.Add(item);
        return item;
    }
}
