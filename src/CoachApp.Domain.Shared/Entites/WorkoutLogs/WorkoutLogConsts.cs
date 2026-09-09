namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// Field length constraints for the <c>WorkoutLog</c> aggregate (log + entries).
/// Shared by the entities, the DTOs and the EF Core configuration.
/// </summary>
public static class WorkoutLogConsts
{
    public const int MaxRepsLength = 32;
    public const int MaxNotesLength = 512;

    /// <summary>Upper bound on entries per logged session (guards against oversized payloads).</summary>
    public const int MaxEntries = 100;
}
