namespace CoachApp.Training;

/// <summary>
/// Business exception error codes for the Training module.
/// Each code maps to a localization key in <c>/Localization/Training/*.json</c>
/// via <c>AbpExceptionLocalizationOptions.MapCodeNamespace("Training", ...)</c>.
/// </summary>
public static class TrainingErrorCodes
{
    public const string TraineeCodeAlreadyExists = "Training:00001";
}
