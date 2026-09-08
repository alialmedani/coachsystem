using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionPlanTemplates;

public class GetNutritionPlanTemplateListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against the template name.</summary>
    public string? Filter { get; set; }
}
