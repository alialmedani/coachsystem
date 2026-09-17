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

    /// <summary>A workout plan scheduled two days on the same weekday (PD9: one scheduled workout per weekday).</summary>
    public const string DuplicateScheduledDay = "CoachApp:00006";

    /// <summary>An attempt to add a second Coach-role user to a tenant that already has one (PD1: one coach per tenant).</summary>
    public const string SecondCoachNotAllowed = "CoachApp:00007";

    /// <summary>A trainee tried to edit/delete a progress entry the coach authored (F4/PD4: trainee may only modify their own).</summary>
    public const string CannotModifyCoachAuthoredProgress = "CoachApp:00008";
}
