using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.NutritionLogs;

public class GetMyNutritionLogListInput : PagedAndSortedResultRequestDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
