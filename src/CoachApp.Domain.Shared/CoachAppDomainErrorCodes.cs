namespace CoachApp;

/// <summary>
/// Business-exception error codes for the application. Each code maps to a
/// localization key in <c>/Localization/CoachApp/*.json</c> via
/// <c>AbpExceptionLocalizationOptions.MapCodeNamespace("CoachApp", ...)</c>
/// configured in <see cref="CoachAppDomainSharedModule"/>.
/// </summary>
public static class CoachAppDomainErrorCodes
{
    public const string TraineeCodeAlreadyExists = "CoachApp:00001";
    public const string TrainingPlanTraineeNotFound = "CoachApp:00002";
    public const string TrainingPlanInvalidDateRange = "CoachApp:00003";
}
