namespace CoachApp.Enums;

/// <summary>Primary muscle group an exercise targets. Stored as a compact <see cref="byte"/>.</summary>
public enum MuscleGroup : byte
{
    Other = 0,
    Chest = 1,
    Back = 2,
    Shoulders = 3,
    Arms = 4,
    Legs = 5,
    Core = 6,
    FullBody = 7,
    Cardio = 8
}
