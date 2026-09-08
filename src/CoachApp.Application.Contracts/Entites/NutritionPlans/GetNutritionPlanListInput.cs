using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionPlans;

public class GetNutritionPlanListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against the plan name.</summary>
    public string? Filter { get; set; }

    public Guid? TraineeId { get; set; }

    public bool? IsActive { get; set; }
}
