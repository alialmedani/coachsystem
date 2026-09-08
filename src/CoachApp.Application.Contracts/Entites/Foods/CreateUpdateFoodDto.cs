using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.Foods;

public class CreateUpdateFoodDto
{
    [Required]
    [StringLength(FoodConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    [StringLength(FoodConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [Range(0, 100000)]
    public decimal ServingSize { get; set; } = 100;

    [Required]
    [StringLength(FoodConsts.MaxServingUnitLength)]
    public string ServingUnit { get; set; } = "g";

    [Range(0, 100000)]
    public decimal Calories { get; set; }

    [Range(0, 100000)]
    public decimal ProteinG { get; set; }

    [Range(0, 100000)]
    public decimal CarbsG { get; set; }

    [Range(0, 100000)]
    public decimal FatG { get; set; }

    public bool IsActive { get; set; } = true;
}
