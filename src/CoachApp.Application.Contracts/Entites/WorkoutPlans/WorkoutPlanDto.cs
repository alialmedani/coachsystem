using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutPlans;

public class WorkoutPlanDto : FullAuditedEntityDto<Guid>
{
    public Guid TraineeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public List<WorkoutDayDto> Days { get; set; } = new();
}
