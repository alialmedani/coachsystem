using System;

namespace CoachApp.Entites.Dashboards;

/// <summary>
/// A single day's nutrition adherence: what the trainee actually consumed (summed from
/// all their nutrition logs on <see cref="Date"/>) compared against the targets of their
/// currently active nutrition plan. All macro math is reused from the nutrition enrichers.
/// </summary>
public class NutritionAdherenceDto
{
    /// <summary>The day this adherence is calculated for (date component only).</summary>
    public DateTime Date { get; set; }

    /// <summary>True when the trainee has an active nutrition plan to compare against.</summary>
    public bool HasActivePlan { get; set; }

    /// <summary>True when at least one nutrition log exists for this day.</summary>
    public bool HasLog { get; set; }

    // Consumed (summed across all logs on the day; 0 when there is no log).
    public decimal ConsumedCalories { get; set; }
    public decimal ConsumedProteinG { get; set; }
    public decimal ConsumedCarbsG { get; set; }
    public decimal ConsumedFatG { get; set; }

    // Active-plan targets (null when there is no active plan).
    public decimal? TargetCalories { get; set; }
    public decimal? TargetProteinG { get; set; }
    public decimal? TargetCarbsG { get; set; }
    public decimal? TargetFatG { get; set; }

    // Consumed / target * 100, rounded to 1 dp. Null when there is no active plan
    // or the target is zero (not applicable). Not capped, so over-eating stays visible.
    public decimal? CaloriesPercent { get; set; }
    public decimal? ProteinPercent { get; set; }
    public decimal? CarbsPercent { get; set; }
    public decimal? FatPercent { get; set; }

    /// <summary>Overall adherence headline = <see cref="CaloriesPercent"/>.</summary>
    public decimal? OverallPercent { get; set; }
}
