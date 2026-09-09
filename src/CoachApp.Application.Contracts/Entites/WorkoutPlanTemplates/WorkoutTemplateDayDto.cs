using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutPlanTemplates;

public class WorkoutTemplateDayDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public DayOfWeek? ScheduledDay { get; set; }

    public List<WorkoutTemplateExerciseDto> Exercises { get; set; } = new();
}
