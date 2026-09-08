using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutPlanTemplates;

public class WorkoutTemplateExerciseDto : EntityDto<Guid>
{
    public Guid ExerciseId { get; set; }

    /// <summary>Enriched from the exercise library for display.</summary>
    public string? ExerciseName { get; set; }

    public int Order { get; set; }

    public int Sets { get; set; }

    public string? Reps { get; set; }

    public decimal? WeightKg { get; set; }

    public int? RestSeconds { get; set; }

    public string? Notes { get; set; }
}
