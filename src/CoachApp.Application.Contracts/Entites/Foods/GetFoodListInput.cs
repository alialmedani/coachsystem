using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.Foods;

public class GetFoodListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against name and description.</summary>
    public string? Filter { get; set; }

    public bool? IsActive { get; set; }
}
