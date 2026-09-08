using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Dashboards;

/// <summary>Trainee input: my workout completion over a date range (trainee = current user).</summary>
public class GetMyWorkoutCompletionInput
{
    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }
}
