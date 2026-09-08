namespace CoachApp.Enums;

/// <summary>
/// The trainee's primary training goal. Stored as a <see cref="byte"/> to keep the
/// column compact and stable against re-ordering of members.
/// </summary>
public enum TrainingGoal : byte
{
    General = 0,
    LoseWeight = 1,
    BuildMuscle = 2,
    Maintain = 3,
    ImproveFitness = 4,
    Strength = 5
}
