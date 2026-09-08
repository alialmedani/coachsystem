using CoachApp.Enums;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.Trainees;

public class GetTraineeListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against user name, first name and last name.</summary>
    public string? Filter { get; set; }

    public TrainingGoal? Goal { get; set; }

    public bool? IsActive { get; set; }
}
