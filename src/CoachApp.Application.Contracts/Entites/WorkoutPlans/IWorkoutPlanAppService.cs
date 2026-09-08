using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.WorkoutPlans;

/// <summary>Coach-facing management of trainees' workout plans (tenant-scoped).</summary>
public interface IWorkoutPlanAppService : IApplicationService
{
    /// <summary>Full plan with days and exercises.</summary>
    Task<WorkoutPlanDto> GetAsync(Guid id);

    /// <summary>Paged plan summaries (no days loaded).</summary>
    Task<PagedResultDto<WorkoutPlanDto>> GetListAsync(GetWorkoutPlanListInput input);

    Task<WorkoutPlanDto> CreateAsync(CreateUpdateWorkoutPlanDto input);

    Task<WorkoutPlanDto> UpdateAsync(Guid id, CreateUpdateWorkoutPlanDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Makes this the trainee's active plan; deactivates their others.</summary>
    Task<WorkoutPlanDto> SetActiveAsync(Guid id);
}
