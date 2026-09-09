using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>
/// Edits a logged nutrition day the trainee owns. Full replace of the entry list, mirroring the
/// workout-log update convention. The plan reference recorded at create time is left unchanged.
/// </summary>
public class UpdateNutritionLogDto
{
    [Required]
    public DateTime Date { get; set; }

    [StringLength(NutritionLogConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    [MaxLength(NutritionLogConsts.MaxEntries)]
    public List<CreateNutritionLogEntryDto> Entries { get; set; } = new();
}
