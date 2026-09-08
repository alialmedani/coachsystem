using System;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Entites.ProgressEntries;

public class GetMyProgressListInput : PagedAndSortedResultRequestDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
