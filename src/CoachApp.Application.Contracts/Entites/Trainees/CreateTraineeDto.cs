using System;
using System.ComponentModel.DataAnnotations;
using CoachApp.Enums;

namespace CoachApp.Entites.Trainees;

/// <summary>
/// Coach-supplied data to create a trainee: the login credentials the coach hands
/// to the trainee (<see cref="UserName"/> + <see cref="Password"/>) plus the
/// coaching profile.
/// </summary>
public class CreateTraineeDto
{
    [Required]
    [StringLength(TraineeConsts.MaxUserNameLength)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [StringLength(TraineeConsts.MaxPasswordLength, MinimumLength = TraineeConsts.MinPasswordLength)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(TraineeConsts.MaxFirstNameLength)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(TraineeConsts.MaxLastNameLength)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(TraineeConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [StringLength(TraineeConsts.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; set; }

    public Gender Gender { get; set; } = Gender.Unspecified;

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    public TrainingGoal Goal { get; set; } = TrainingGoal.General;

    [Range(0, 300)]
    public decimal? HeightCm { get; set; }

    [Range(0, 500)]
    public decimal? StartWeightKg { get; set; }

    [Range(0, 500)]
    public decimal? TargetWeightKg { get; set; }

    public bool IsActive { get; set; } = true;
}
