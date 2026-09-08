using System;
using System.ComponentModel.DataAnnotations;
using CoachApp.Enums;

namespace CoachApp.Entites.Trainees;

public class CreateUpdateTraineeDto
{
    [Required]
    [StringLength(TraineeConsts.MaxCodeLength)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(TraineeConsts.MaxFirstNameLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(TraineeConsts.MaxLastNameLength)]
    public string LastName { get; set; } = string.Empty;

    public Gender Gender { get; set; } = Gender.Unspecified;

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [EmailAddress]
    [StringLength(TraineeConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [StringLength(TraineeConsts.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; set; }

    [StringLength(TraineeConsts.MaxAddressLength)]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;
}
