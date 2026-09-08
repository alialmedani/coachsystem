using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Dashboards;

/// <summary>
/// Coach input for the combined dashboard: <see cref="Date"/> drives nutrition adherence,
/// <see cref="FromDate"/>/<see cref="ToDate"/> drive workout completion.
/// </summary>
public class GetTraineeDashboardInput
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }
}
