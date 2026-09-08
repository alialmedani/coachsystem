using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// A session the trainee logs for themselves (the trainee is the current user, so
/// there is no TraineeId here).
/// </summary>
public class CreateWorkoutLogDto
{
    public Guid? WorkoutPlanId { get; set; }

    public Guid? WorkoutDayId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(WorkoutLogConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<CreateWorkoutLogEntryDto> Entries { get; set; } = new();
}
