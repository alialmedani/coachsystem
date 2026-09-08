namespace CoachApp.Entites.TrainingPlans;

/// <summary>
/// Field length constraints for the <c>TrainingPlan</c> aggregate. Shared by the
/// entity (invariant checks), the DTOs (data-annotation validation) and the
/// EF Core model configuration so the three never drift apart.
/// </summary>
public static class TrainingPlanConsts
{
    public const int MaxTitleLength = 128;
    public const int MaxDescriptionLength = 1024;
}
