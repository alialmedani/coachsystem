namespace CoachApp;

/// <summary>
/// Business-exception error codes for the application. Each code maps to a
/// localization key in <c>/Localization/CoachApp/*.json</c> via
/// <c>AbpExceptionLocalizationOptions.MapCodeNamespace("CoachApp", ...)</c>
/// configured in <see cref="CoachAppDomainSharedModule"/>. Throw them as
/// <c>BusinessException(CoachAppDomainErrorCodes.X)</c> so API clients get a
/// stable machine-readable <c>error.code</c> alongside the localized message.
/// </summary>
public static class CoachAppDomainErrorCodes
{
    public const string TraineeNotFound = "CoachApp:00001";
    public const string ExercisesNotFound = "CoachApp:00002";
    public const string FoodsNotFound = "CoachApp:00003";
    public const string ExerciseInUse = "CoachApp:00004";
    public const string FoodInUse = "CoachApp:00005";
}
