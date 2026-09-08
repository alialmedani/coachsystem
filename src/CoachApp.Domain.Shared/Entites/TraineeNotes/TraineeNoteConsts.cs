namespace CoachApp.Entites.TraineeNotes;

/// <summary>
/// Field length constraints for the <c>TraineeNote</c> aggregate. Shared by the
/// entity, the DTOs and the EF Core configuration.
/// </summary>
public static class TraineeNoteConsts
{
    public const int MaxTextLength = 2000;
}
