using System;
using System.ComponentModel.DataAnnotations;

namespace CoachApp.Entites.TraineeNotes;

public class CreateUpdateTraineeNoteDto
{
    [Required]
    public Guid TraineeId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(TraineeNoteConsts.MaxTextLength)]
    public string Text { get; set; } = string.Empty;
}
