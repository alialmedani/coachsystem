using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own sessions.</summary>
public interface IMyWorkoutLogAppService : IApplicationService
{
    Task<WorkoutLogDto> CreateAsync(CreateWorkoutLogDto input);

    Task<PagedResultDto<WorkoutLogDto>> GetListAsync(GetMyWorkoutLogListInput input);

    Task<WorkoutLogDto> GetAsync(Guid id);

    Task DeleteAsync(Guid id);
}
