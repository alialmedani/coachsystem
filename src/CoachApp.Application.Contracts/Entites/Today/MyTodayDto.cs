using System;
using System.Collections.Generic;
using CoachApp.Entites.Dashboards;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;

namespace CoachApp.Entites.Today;

/// <summary>
/// The trainee's "Today" home screen: what to train today, what to eat, how today is tracking,
/// and whether the session has already been logged. All data is for the signed-in trainee only.
/// </summary>
public class MyTodayDto
{
    public DateTime Date { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    // ── Workout ──────────────────────────────────────────────────────────────
    public bool HasActiveWorkoutPlan { get; set; }

    public Guid? WorkoutPlanId { get; set; }

    /// <summary>True when no workout day is scheduled for today's weekday.</summary>
    public bool IsRestDay { get; set; }

    /// <summary>Today's scheduled workout day(s) (exercises included/enriched), ordered by Order.</summary>
    public List<WorkoutDayDto> ScheduledWorkoutDays { get; set; } = new();

    public bool AlreadyLoggedWorkoutToday { get; set; }

    public Guid? LatestWorkoutLogId { get; set; }

    /// <summary>The most recent workout log for the date (D5), if any.</summary>
    public WorkoutLogDto? LatestWorkoutLog { get; set; }

    // ── Nutrition ────────────────────────────────────────────────────────────
    public bool HasActiveNutritionPlan { get; set; }

    /// <summary>The active nutrition plan (meals + targets), enriched; null if none.</summary>
    public NutritionPlanDto? NutritionPlan { get; set; }

    /// <summary>Today's consumed-vs-target adherence (always populated, even with no plan/log).</summary>
    public NutritionAdherenceDto NutritionAdherence { get; set; } = new();

    public bool AlreadyLoggedNutritionToday { get; set; }
}
