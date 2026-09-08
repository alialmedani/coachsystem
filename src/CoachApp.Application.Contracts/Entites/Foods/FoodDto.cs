using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.Foods;

public class FoodDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal ServingSize { get; set; }

    public string ServingUnit { get; set; } = "g";

    public decimal Calories { get; set; }

    public decimal ProteinG { get; set; }

    public decimal CarbsG { get; set; }

    public decimal FatG { get; set; }

    public bool IsActive { get; set; }
}
