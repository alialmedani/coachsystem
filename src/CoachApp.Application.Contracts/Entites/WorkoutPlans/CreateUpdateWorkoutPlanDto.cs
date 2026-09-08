using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutPlans;

public class CreateUpdateWorkoutPlanDto
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    [StringLength(WorkoutPlanConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(WorkoutPlanConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public List<CreateUpdateWorkoutDayDto> Days { get; set; } = new();
}
