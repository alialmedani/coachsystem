using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Trainees;

/// <summary>
/// PD6: the fields a trainee may edit on their OWN profile — contact details and birthdate only.
/// The user name is immutable (a login credential) and every coaching field (goal, height, start/
/// target weight, active state) stays coach-owned and is not accepted here. "Current weight" is not
/// a profile column in this domain; the trainee records it as a progress entry (see MyProgress).
/// </summary>
public class UpdateMyProfileDto
{
    [StringLength(TraineeConsts.MaxPhoneNumberLength)]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [StringLength(TraineeConsts.MaxEmailLength)]
    public string? Email { get; set; }

    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }
}
