using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.ProgressEntries;

/// <summary>Coach view of a specific trainee's progress entries.</summary>
public class GetProgressEntryListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid TraineeId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
