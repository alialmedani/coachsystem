namespace CoachApp;

/// <summary>
/// Name prefixes for the two permission personas, used by
/// <c>CoachAppDataSeedContributor</c> to grant a whole subtree to a role without
/// referencing the Application.Contracts <c>CoachAppPermissions</c> constants
/// (which the Domain layer does not reference). Kept in sync with
/// <c>CoachAppPermissions.GroupName</c> ("CoachApp").
/// </summary>
public static class CoachAppPermissionPrefixes
{
    public const string Coach = "CoachApp.Coach";
    public const string Trainee = "CoachApp.Trainee";
}
