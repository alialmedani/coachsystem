namespace CoachApp.Enums;

/// <summary>Equipment an exercise requires. Stored as a compact <see cref="byte"/>.</summary>
public enum Equipment : byte
{
    None = 0,
    Bodyweight = 1,
    Barbell = 2,
    Dumbbell = 3,
    Machine = 4,
    Cable = 5,
    Kettlebell = 6,
    ResistanceBand = 7,
    Other = 8
}
