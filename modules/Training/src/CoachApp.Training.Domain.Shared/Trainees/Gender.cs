namespace CoachApp.Training.Trainees;

/// <summary>
/// Trainee gender. Stored as a <see cref="byte"/> to keep the column compact
/// and stable against re-ordering of members.
/// </summary>
public enum Gender : byte
{
    Unspecified = 0,
    Male = 1,
    Female = 2
}
