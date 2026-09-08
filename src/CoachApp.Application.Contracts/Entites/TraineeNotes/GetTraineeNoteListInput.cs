using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.TraineeNotes;

/// <summary>Coach view of a specific trainee's notes.</summary>
public class GetTraineeNoteListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid TraineeId { get; set; }
}
