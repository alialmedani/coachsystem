using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>Coach view of a specific trainee's logged nutrition days.</summary>
public class GetNutritionLogListInput : PagedAndSortedResultRequestDto
{
    [Required]
    public Guid TraineeId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
