namespace CoachApp.Training;

/// <summary>
/// Central database configuration for the Training module.
/// Table prefix "App" and a null schema keep Training tables aligned with the
/// host <c>CoachApp</c> database (see <c>CoachAppConsts.DbTablePrefix</c>).
/// </summary>
public static class TrainingDbProperties
{
    public static string DbTablePrefix { get; set; } = "App";

    public static string? DbSchema { get; set; } = null;

    /// <summary>
    /// Logical connection string name for the module. The host replaces
    /// <c>ITrainingDbContext</c> with its own DbContext, so at runtime this
    /// falls back to the host "Default" connection string.
    /// </summary>
    public const string ConnectionStringName = "Training";
}
