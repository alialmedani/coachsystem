using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutLogs;

public class WorkoutLogDto : FullAuditedEntityDto<Guid>
{
    public Guid TraineeId { get; set; }

    public Guid? WorkoutPlanId { get; set; }

    public Guid? WorkoutDayId { get; set; }

    public DateTime Date { get; set; }

    public string? Notes { get; set; }

    public List<WorkoutLogEntryDto> Entries { get; set; } = new();
}
