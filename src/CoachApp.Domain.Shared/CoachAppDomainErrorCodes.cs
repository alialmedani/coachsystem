namespace CoachApp;

/// <summary>
/// Business-exception error codes for the application. Each code maps to a
/// localization key in <c>/Localization/CoachApp/*.json</c> via
/// <c>AbpExceptionLocalizationOptions.MapCodeNamespace("CoachApp", ...)</c>
/// configured in <see cref="CoachAppDomainSharedModule"/>.
/// </summary>
public static class CoachAppDomainErrorCodes
{
    // Reserve codes here as business rules are added, e.g.:
    // public const string SomeRuleViolated = "CoachApp:00001";
}
