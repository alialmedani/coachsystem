using Volo.Abp.Identity;

namespace CoachApp;

public static class CoachAppConsts
{
    public const string DbTablePrefix = "App";
    public const string? DbSchema = null;
    public const string AdminEmailDefaultValue = IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = "1q2w3E*";

    /// <summary>
    /// Name of the demo coach tenant seeded for local development so the
    /// tenant-per-coach model is testable out of the box. Its auto-seeded admin
    /// user acts as the coach.
    /// </summary>
    public const string DemoCoachTenantName = "demo";
}
