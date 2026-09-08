namespace CoachApp.Entites.Foods;

/// <summary>
/// Field length constraints for the <c>Food</c> aggregate. Shared by the entity,
/// the DTOs and the EF Core configuration.
/// </summary>
public static class FoodConsts
{
    public const int MaxNameLength = 128;
    public const int MaxDescriptionLength = 512;
    public const int MaxServingUnitLength = 16;
}
