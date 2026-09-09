using System;
using System.Threading.Tasks;
using CoachApp.Entites.Dashboards;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.Dashboards;

/// <summary>
/// Trainee-facing analytics for the signed-in trainee. The trainee id is always resolved from
/// the current user (never trusted from the client), so a trainee only ever sees their own numbers.
/// </summary>
[Authorize(CoachAppPermissions.Trainee.MyDashboard.Default)]
public class MyDashboardAppService : MyTraineeAppServiceBase, IMyDashboardAppService
{
    private readonly IRepository<NutritionLog, Guid> _nutritionLogRepository;
    private readonly IRepository<NutritionPlan, Guid> _nutritionPlanRepository;
    private readonly IRepository<WorkoutLog, Guid> _workoutLogRepository;
    private readonly IRepository<WorkoutPlan, Guid> _workoutPlanRepository;
    private readonly IRepository<Food, Guid> _foodRepository;

    public MyDashboardAppService(
        IRepository<NutritionLog, Guid> nutritionLogRepository,
        IRepository<NutritionPlan, Guid> nutritionPlanRepository,
        IRepository<WorkoutLog, Guid> workoutLogRepository,
        IRepository<WorkoutPlan, Guid> workoutPlanRepository,
        IRepository<Food, Guid> foodRepository)
    {
        _nutritionLogRepository = nutritionLogRepository;
        _nutritionPlanRepository = nutritionPlanRepository;
        _workoutLogRepository = workoutLogRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _foodRepository = foodRepository;
    }

    public virtual async Task<NutritionAdherenceDto> GetNutritionAdherenceAsync(GetMyNutritionAdherenceInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        return await DashboardCalculator.ComputeNutritionAdherenceAsync(
            traineeId, input.Date,
            _nutritionLogRepository, _nutritionPlanRepository, _foodRepository,
            ObjectMapper, AsyncExecuter);
    }

    public virtual async Task<WorkoutCompletionDto> GetWorkoutCompletionAsync(GetMyWorkoutCompletionInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        return await DashboardCalculator.ComputeWorkoutCompletionAsync(
            traineeId, input.FromDate, input.ToDate,
            _workoutLogRepository, _workoutPlanRepository,
            AsyncExecuter);
    }

    public virtual async Task<TraineeDashboardDto> GetSummaryAsync(GetMyDashboardInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        return new TraineeDashboardDto
        {
            TraineeId = traineeId,
            NutritionAdherence = await DashboardCalculator.ComputeNutritionAdherenceAsync(
                traineeId, input.Date,
                _nutritionLogRepository, _nutritionPlanRepository, _foodRepository,
                ObjectMapper, AsyncExecuter),
            WorkoutCompletion = await DashboardCalculator.ComputeWorkoutCompletionAsync(
                traineeId, input.FromDate, input.ToDate,
                _workoutLogRepository, _workoutPlanRepository,
                AsyncExecuter)
        };
    }
}
