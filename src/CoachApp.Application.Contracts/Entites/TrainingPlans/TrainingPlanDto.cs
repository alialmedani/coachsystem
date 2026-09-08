using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.TrainingPlans;

public class TrainingPlanDto : FullAuditedEntityDto<Guid>
{
    public Guid TraineeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }
}
