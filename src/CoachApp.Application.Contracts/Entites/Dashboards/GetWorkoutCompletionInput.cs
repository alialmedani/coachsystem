using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Dashboards;

/// <summary>Coach input: workout completion for a specific trainee over a date range.</summary>
public class GetWorkoutCompletionInput
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }
}
