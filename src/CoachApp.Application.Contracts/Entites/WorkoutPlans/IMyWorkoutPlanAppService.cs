using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.WorkoutPlans;

/// <summary>Trainee-facing: the signed-in trainee reads their own workout plans.</summary>
public interface IMyWorkoutPlanAppService : IApplicationService
{
    /// <summary>My plan summaries (no days).</summary>
    Task<List<WorkoutPlanDto>> GetListAsync();

    /// <summary>One of my plans, with days and exercises.</summary>
    Task<WorkoutPlanDto> GetAsync(Guid id);

    /// <summary>My active plan (with days and exercises), or null if none.</summary>
    Task<WorkoutPlanDto?> GetActiveAsync();
}
