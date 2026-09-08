namespace CoachApp.Entites.NutritionPlans;

/// <summary>
/// Field length constraints for the <c>NutritionPlan</c> aggregate (plan + meals +
/// items). Shared by the entities, the DTOs and the EF Core configuration.
/// </summary>
public static class NutritionPlanConsts
{
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 512;
    public const int MaxMealNameLength = 64;
}
