using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.NutritionPlans;

public class CreateUpdateMealItemDto
{
    [Required]
    public Guid FoodId { get; set; }

    public int Order { get; set; }

    [Range(0, 10000)]
    public decimal Quantity { get; set; } = 1;
}
