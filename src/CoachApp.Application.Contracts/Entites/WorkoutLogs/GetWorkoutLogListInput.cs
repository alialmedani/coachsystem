using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>Coach view of a specific trainee's logged sessions.</summary>
public class GetWorkoutLogListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid TraineeId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
