namespace CoachApp;

/// <summary>
/// Well-known role names seeded by <c>CoachAppDataSeedContributor</c>.
/// A <see cref="Coach"/> manages trainees, libraries and plans (tenant-scoped);
/// a <see cref="Trainee"/> reads their own plans and logs their activity.
/// </summary>
public static class CoachAppRoles
{
    public const string Coach = "Coach";
    public const string Trainee = "Trainee";
}
