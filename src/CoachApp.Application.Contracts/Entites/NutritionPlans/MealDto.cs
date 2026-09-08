using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionPlans;

public class MealDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<MealItemDto> Items { get; set; } = new();
}
