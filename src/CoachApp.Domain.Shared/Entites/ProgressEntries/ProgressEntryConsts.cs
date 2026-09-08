namespace CoachApp.Entites.ProgressEntries;

/// <summary>
/// Field length constraints for the <c>ProgressEntry</c> aggregate. Shared by the
/// entity, the DTOs and the EF Core configuration.
/// </summary>
public static class ProgressEntryConsts
{
    public const int MaxNotesLength = 512;
}
