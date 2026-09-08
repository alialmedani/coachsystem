using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.TrainingPlans;

public class GetTrainingPlanListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against title and description.</summary>
    public string? Filter { get; set; }

    public Guid? TraineeId { get; set; }

    public bool? IsActive { get; set; }
}
