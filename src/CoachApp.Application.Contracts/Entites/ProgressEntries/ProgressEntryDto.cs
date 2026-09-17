using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.ProgressEntries;

public class ProgressEntryDto : FullAuditedEntityDto<Guid>
{
    public Guid TraineeId { get; set; }

    public DateTime Date { get; set; }

    public decimal? WeightKg { get; set; }

    public decimal? BodyFatPercent { get; set; }

    public decimal? ChestCm { get; set; }

    public decimal? WaistCm { get; set; }

    public decimal? HipsCm { get; set; }

    public decimal? ArmCm { get; set; }

    public decimal? ThighCm { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// F4/PD4 attribution: true when this entry was recorded by the coach rather than the
    /// signed-in trainee. Set on the trainee-facing "my" endpoints (a trainee may only edit or
    /// delete entries they authored themselves). Always false on the coach-facing endpoints.
    /// </summary>
    public bool IsCoachAuthored { get; set; }
}
