using CoachApp.Enums;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.Exercises;

public class GetExerciseListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Free-text filter matched against name and description.</summary>
    public string? Filter { get; set; }

    public MuscleGroup? TargetMuscle { get; set; }

    public Equipment? Equipment { get; set; }

    public bool? IsActive { get; set; }
}
