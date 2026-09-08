using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>Coach-facing: read a trainee's logged sessions (progress/adherence tracking).</summary>
public interface IWorkoutLogAppService : IApplicationService
{
    Task<PagedResultDto<WorkoutLogDto>> GetListAsync(GetWorkoutLogListInput input);

    Task<WorkoutLogDto> GetAsync(Guid id);
}
