using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutPlans;

public class WorkoutDayDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public int Order { get; set; }

    public List<WorkoutExerciseDto> Exercises { get; set; } = new();
}
