using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.NutritionPlans;

public class CreateUpdateNutritionPlanDto
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    [StringLength(NutritionPlanConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(NutritionPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public List<CreateUpdateMealDto> Meals { get; set; } = new();
}
