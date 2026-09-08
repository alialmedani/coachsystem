using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>
/// A day the trainee logs for themselves (the trainee is the current user, so there
/// is no TraineeId here).
/// </summary>
public class CreateNutritionLogDto
{
    public Guid? NutritionPlanId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [StringLength(NutritionLogConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    public List<CreateNutritionLogEntryDto> Entries { get; set; } = new();
}
