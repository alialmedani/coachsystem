using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Trainees;

/// <summary>
/// Coach-supplied new password when resetting a trainee's login. Only the new password is
/// accepted here — the trainee's user name is not changed and no current password is required.
/// The value is validated by the standard ABP Identity password policy on reset.
/// </summary>
public class ResetTraineePasswordDto
{
    [Required]
    [StringLength(TraineeConsts.MaxPasswordLength, MinimumLength = TraineeConsts.MinPasswordLength)]
    public string NewPassword { get; set; } = string.Empty;
}
