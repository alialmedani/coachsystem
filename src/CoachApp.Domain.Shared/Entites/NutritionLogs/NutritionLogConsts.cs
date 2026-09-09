namespace CoachApp.Entites.NutritionLogs;

/// <summary>
/// Field length constraints for the <c>NutritionLog</c> aggregate (log + entries).
/// Shared by the entities, the DTOs and the EF Core configuration.
/// </summary>
public static class NutritionLogConsts
{
    public const int MaxNotesLength = 512;

    /// <summary>Upper bound on entries per logged day (matches WorkoutLogConsts.MaxEntries).</summary>
    public const int MaxEntries = 100;
}
