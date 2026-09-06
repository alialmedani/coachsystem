using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Training.Trainees;

public class TraineeDto : FullAuditedEntityDto<Guid>
{
    public string Code { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    public DateTime? BirthDate { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }
}
