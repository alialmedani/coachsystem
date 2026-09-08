using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.NutritionPlans;

public class CreateUpdateMealDto
{
    [Required]
    [StringLength(NutritionPlanConsts.MaxMealNameLength)]
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<CreateUpdateMealItemDto> Items { get; set; } = new();
}
