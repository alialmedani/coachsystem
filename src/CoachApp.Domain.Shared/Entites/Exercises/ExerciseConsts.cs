namespace CoachApp.Entites.Exercises;

/// <summary>
/// Field length constraints for the <c>Exercise</c> aggregate. Shared by the
/// entity, the DTOs and the EF Core model configuration so the three never drift.
/// </summary>
public static class ExerciseConsts
{
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 512;
    public const int MaxInstructionsLength = 2000;
    public const int MaxMediaUrlLength = 512;
}
