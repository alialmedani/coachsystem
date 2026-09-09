using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// Edits a logged session the trainee owns (e.g. adjusting the actual sets/reps/weight they
/// performed). Full replace of the entry list, mirroring the plan clear-and-rebuild convention.
/// </summary>
public class UpdateWorkoutLogDto
{
    [Required]
    public DateTime Date { get; set; }

    [StringLength(WorkoutLogConsts.MaxNotesLength)]
    public string? Notes { get; set; }

    [MaxLength(WorkoutLogConsts.MaxEntries)]
    public List<UpdateWorkoutLogEntryDto> Entries { get; set; } = new();
}
