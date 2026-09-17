using System;

namespace CoachApp.Entites.Dashboards;

/// <summary>
/// F5/PD5: nutrition adherence aggregated over a date range (e.g. the trailing week) rather than a
/// single day, so a coach/trainee can see a weekly average instead of only "today". Consumption is
/// summed across every log in the range and compared to the active plan's daily calorie target;
/// <see cref="AverageCaloriesPercent"/> averages over the days actually logged so occasional missed
/// days don't silently drag the number to zero. <see cref="DaysLogged"/> vs <see cref="DaysInRange"/>
/// conveys coverage separately from quality.
/// </summary>
public class NutritionAdherenceRangeDto
{
    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    /// <summary>Inclusive number of days in the range.</summary>
    public int DaysInRange { get; set; }

    /// <summary>Distinct days in the range that have at least one nutrition log.</summary>
    public int DaysLogged { get; set; }

    /// <summary>True when the trainee has an active nutrition plan to compare against.</summary>
    public bool HasActivePlan { get; set; }

    /// <summary>Total calories consumed across all logs in the range.</summary>
    public decimal ConsumedCaloriesTotal { get; set; }

    /// <summary>The active plan's daily calorie target (explicit target, else the plan's meal totals). Null when no active plan.</summary>
    public decimal? TargetCaloriesPerDay { get; set; }

    /// <summary>Average daily calorie adherence over the logged days, rounded to 1 dp. Null when nothing is logged or there is no target.</summary>
    public decimal? AverageCaloriesPercent { get; set; }
}
