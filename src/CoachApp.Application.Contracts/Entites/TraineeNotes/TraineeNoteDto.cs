using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.TraineeNotes;

public class TraineeNoteDto : FullAuditedEntityDto<Guid>
{
    public Guid TraineeId { get; set; }

    public DateTime Date { get; set; }

    public string Text { get; set; } = string.Empty;
}
