using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.WorkoutLogs;

public class GetMyWorkoutLogListInput : PagedAndSortedResultRequestDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
