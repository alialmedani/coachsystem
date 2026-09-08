using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutPlanTemplates;

public class GetWorkoutPlanTemplateListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against the template name.</summary>
    public string? Filter { get; set; }
}
