using System;
using CoachApp.Enums;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.Trainees;

public class TraineeDto : FullAuditedEntityDto<Guid>
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public Gender Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public TrainingGoal Goal { get; set; }

    public decimal? HeightCm { get; set; }

    public decimal? StartWeightKg { get; set; }

    public decimal? TargetWeightKg { get; set; }

    public bool IsActive { get; set; }
}
