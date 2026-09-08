using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutPlanTemplates;

public class WorkoutPlanTemplateDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<WorkoutTemplateDayDto> Days { get; set; } = new();
}
