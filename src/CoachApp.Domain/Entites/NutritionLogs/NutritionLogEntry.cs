using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>
/// One food the trainee actually consumed in a logged day, with the quantity
/// (number of the food's servings). Calories/macros are computed from the food
/// library at read time. Child entity of the NutritionLog aggregate.
/// </summary>
public class NutritionLogEntry : Entity<Guid>
{
    public virtual Guid NutritionLogId { get; protected set; }

    public virtual Guid FoodId { get; set; }

    public virtual int Order { get; set; }

    public virtual decimal Quantity { get; set; }

    public virtual string? Notes { get; set; }

    protected NutritionLogEntry()
    {
    }

    public NutritionLogEntry(
        Guid id,
        Guid nutritionLogId,
        Guid foodId,
        int order,
        decimal quantity,
        string? notes = null)
        : base(id)
    {
        NutritionLogId = nutritionLogId;
        FoodId = foodId;
        Order = order;
        Quantity = quantity;
        Notes = Check.Length(notes, nameof(notes), NutritionLogConsts.MaxNotesLength);
    }
}
