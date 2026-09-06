using Volo.Abp.Application.Dtos;

namespace CoachApp.Training.Trainees;

public class GetTraineeListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against code, first name and last name.</summary>
    public string? Filter { get; set; }

    public Gender? Gender { get; set; }

    public bool? IsActive { get; set; }
}
