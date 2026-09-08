using System.ComponentModel.DataAnnotations;
using CoachApp.Enums;

namespace CoachApp.Entites.Exercises;

public class CreateUpdateExerciseDto
{
    [Required]
    [StringLength(ExerciseConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(ExerciseConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(ExerciseConsts.MaxInstructionsLength)]
    public string? Instructions { get; set; }

    public MuscleGroup TargetMuscle { get; set; } = MuscleGroup.Other;

    public Equipment Equipment { get; set; } = Equipment.None;

    [Url]
    [StringLength(ExerciseConsts.MaxMediaUrlLength)]
    public string? VideoUrl { get; set; }

    [Url]
    [StringLength(ExerciseConsts.MaxMediaUrlLength)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
