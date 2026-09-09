using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own sessions.</summary>
public interface IMyWorkoutLogAppService : IApplicationService
{
    Task<WorkoutLogDto> CreateAsync(CreateWorkoutLogDto input);

    /// <summary>"Log This Session": creates a log snapshotted from a day of one of the trainee's own plans.</summary>
    Task<WorkoutLogDto> CreateFromDayAsync(CreateWorkoutLogFromDayDto input);

    Task<PagedResultDto<WorkoutLogDto>> GetListAsync(GetMyWorkoutLogListInput input);

    Task<WorkoutLogDto> GetAsync(Guid id);

    Task<WorkoutLogDto> UpdateAsync(Guid id, UpdateWorkoutLogDto input);

    Task DeleteAsync(Guid id);
}
