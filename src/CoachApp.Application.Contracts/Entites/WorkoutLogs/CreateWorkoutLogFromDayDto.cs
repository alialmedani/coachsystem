using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// "Log This Session": the trainee starts a workout log from a day of one of their assigned
/// plans. The server snapshots that day's prescribed exercises into the log, so the trainee
/// doesn't re-enter them. The trainee is the current user (no TraineeId here).
/// </summary>
public class CreateWorkoutLogFromDayDto
{
    [Required]
    public Guid WorkoutDayId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(WorkoutLogConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
