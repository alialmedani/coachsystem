namespace CoachApp.Entites.WorkoutPlans;

/// <summary>
/// Field length constraints for the <c>WorkoutPlan</c> aggregate (plan + days +
/// exercises). Shared by the entities, the DTOs and the EF Core configuration.
/// </summary>
public static class WorkoutPlanConsts
{
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 512;
    public const int MaxDayNameLength = 64;
    public const int MaxRepsLength = 32;
    public const int MaxNotesLength = 512;
}
