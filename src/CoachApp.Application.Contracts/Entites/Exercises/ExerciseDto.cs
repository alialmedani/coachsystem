using System;
using CoachApp.Enums;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.Exercises;

public class ExerciseDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Instructions { get; set; }

    public MuscleGroup TargetMuscle { get; set; }

    public Equipment Equipment { get; set; }

    public string? VideoUrl { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
}
