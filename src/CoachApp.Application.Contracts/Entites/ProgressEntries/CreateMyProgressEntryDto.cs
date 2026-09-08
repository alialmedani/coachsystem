using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.ProgressEntries;

/// <summary>Trainee logs a progress entry for themselves (no TraineeId — it is the current user).</summary>
public class CreateMyProgressEntryDto
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
