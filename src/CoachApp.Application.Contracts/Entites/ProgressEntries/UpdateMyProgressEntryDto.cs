using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.ProgressEntries;

/// <summary>
/// F3/PD4: the trainee edits one of their OWN progress entries. Same shape as
/// <see cref="CreateMyProgressEntryDto"/> — no TraineeId (it is the current user) and the entry
/// must have been authored by the trainee (coach-authored entries are read-only to the trainee).
/// </summary>
public class UpdateMyProgressEntryDto
{
    [Required]
    public DateTime Date { get; set; }

    [Range(0, 500)] public decimal? WeightKg { get; set; }
    [Range(0, 100)] public decimal? BodyFatPercent { get; set; }
    [Range(0, 500)] public decimal? ChestCm { get; set; }
    [Range(0, 500)] public decimal? WaistCm { get; set; }
    [Range(0, 500)] public decimal? HipsCm { get; set; }
    [Range(0, 500)] public decimal? ArmCm { get; set; }
    [Range(0, 500)] public decimal? ThighCm { get; set; }

    [StringLength(ProgressEntryConsts.MaxNotesLength)]
    public string? Notes { get; set; }
}
