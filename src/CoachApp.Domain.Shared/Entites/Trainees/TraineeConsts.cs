namespace CoachApp.Entites.Trainees;

/// <summary>
/// Field length constraints for the <c>Trainee</c> aggregate. Shared by the
/// entity (invariant checks), the DTOs (data-annotation validation) and the
/// EF Core model configuration so the three never drift apart.
/// </summary>
public static class TraineeConsts
{
    public const int MaxCodeLength = 32;
    public const int MaxFirstNameLength = 64;
    public const int MaxLastNameLength = 64;
    public const int MaxEmailLength = 256;
    public const int MaxPhoneNumberLength = 32;
    public const int MaxAddressLength = 512;
}
