using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionLogs;

public class NutritionLogDto : FullAuditedEntityDto<Guid>
{
    public Guid TraineeId { get; set; }

    public Guid? NutritionPlanId { get; set; }

    public DateTime Date { get; set; }

    public string? Notes { get; set; }

    public List<NutritionLogEntryDto> Entries { get; set; } = new();

    // Totals summed from all entries (enriched in the app service).
    public decimal TotalCalories { get; set; }
    public decimal TotalProteinG { get; set; }
    public decimal TotalCarbsG { get; set; }
    public decimal TotalFatG { get; set; }
}
