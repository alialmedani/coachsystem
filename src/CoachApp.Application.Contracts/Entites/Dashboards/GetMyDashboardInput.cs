using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Dashboards;

/// <summary>Trainee input for the combined dashboard (trainee = current user).</summary>
public class GetMyDashboardInput
{
    [Required]
    public DateTime Date { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }
}
