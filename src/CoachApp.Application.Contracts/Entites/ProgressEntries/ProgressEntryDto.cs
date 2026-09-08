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
}
