using System;

namespace CoachApp.Entites.Dashboards;

/// <summary>
/// Workout completion over a date range: how many days the trainee logged a workout versus
/// how many were planned. Planned = (number of days in the active workout plan) per week,
/// multiplied by the number of (partial) weeks in the range.
/// </summary>
public class WorkoutCompletionDto
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    /// <summary>True when the trainee has an active workout plan to compare against.</summary>
    public bool HasActivePlan { get; set; }

    /// <summary>Days in the active plan (the planned cadence per week); null when no active plan.</summary>
    public int? PlannedPerWeek { get; set; }

    /// <summary>Number of (partial) weeks in the range = ceil(inclusiveDays / 7).</summary>
    public int Weeks { get; set; }

    /// <summary><see cref="PlannedPerWeek"/> * <see cref="Weeks"/>; null when no active plan.</summary>
    public int? PlannedSessions { get; set; }

    /// <summary>Distinct days on which the trainee logged a workout within the range.</summary>
    public int CompletedSessions { get; set; }

    /// <summary>Completed / planned * 100, rounded to 1 dp; null when no active plan. Not capped.</summary>
    public decimal? CompletionPercent { get; set; }
}
