using System;

namespace CoachApp.Entites.Dashboards;

/// <summary>
/// A combined snapshot for a trainee: nutrition adherence for a given day plus workout
/// completion over a given range. Both sub-sections may be present independently.
/// </summary>
public class TraineeDashboardDto
{
    public Guid TraineeId { get; set; }

    public NutritionAdherenceDto? NutritionAdherence { get; set; }

    public WorkoutCompletionDto? WorkoutCompletion { get; set; }
}
