using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionPlanTemplates;

public class NutritionTemplateMealDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<NutritionTemplateItemDto> Items { get; set; } = new();
}
