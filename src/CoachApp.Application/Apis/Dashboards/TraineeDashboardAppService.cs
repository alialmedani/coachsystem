using System;
using System.Threading.Tasks;
using CoachApp.Entites.Dashboards;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.Dashboards;

/// <summary>
/// Coach-facing analytics for a given trainee: per-day nutrition adherence and workout
/// completion over a range. Read-only, guarded by the shared tracking permission; tenant
/// isolation is automatic via ABP's tenant filter on every repository.
/// </summary>
[Authorize(CoachAppPermissions.Coach.Tracking.Default)]
public class TraineeDashboardAppService : CoachAppAppService, ITraineeDashboardAppService
{
    private readonly IRepository<NutritionLog, Guid> _nutritionLogRepository;
    private readonly IRepository<NutritionPlan, Guid> _nutritionPlanRepository;
    private readonly IRepository<WorkoutLog, Guid> _workoutLogRepository;
    private readonly IRepository<WorkoutPlan, Guid> _workoutPlanRepository;
    private readonly IRepository<Food, Guid> _foodRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public TraineeDashboardAppService(
        IRepository<NutritionLog, Guid> nutritionLogRepository,
        IRepository<NutritionPlan, Guid> nutritionPlanRepository,
        IRepository<WorkoutLog, Guid> workoutLogRepository,
        IRepository<WorkoutPlan, Guid> workoutPlanRepository,
        IRepository<Food, Guid> foodRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _nutritionLogRepository = nutritionLogRepository;
        _nutritionPlanRepository = nutritionPlanRepository;
        _workoutLogRepository = workoutLogRepository;
        _workoutPlanRepository = workoutPlanRepository;
        _foodRepository = foodRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<NutritionAdherenceDto> GetNutritionAdherenceAsync(GetNutritionAdherenceInput input)
    {
        await CheckTraineeExistsAsync(input.TraineeId);
        return await DashboardCalculator.ComputeNutritionAdherenceAsync(
            input.TraineeId, input.Date,
            _nutritionLogRepository, _nutritionPlanRepository, _foodRepository,
            ObjectMapper, AsyncExecuter);
    }

    public virtual async Task<WorkoutCompletionDto> GetWorkoutCompletionAsync(GetWorkoutCompletionInput input)
    {
        await CheckTraineeExistsAsync(input.TraineeId);
        return await DashboardCalculator.ComputeWorkoutCompletionAsync(
            input.TraineeId, input.FromDate, input.ToDate,
            _workoutLogRepository, _workoutPlanRepository,
            AsyncExecuter);
    }

    public virtual async Task<TraineeDashboardDto> GetSummaryAsync(GetTraineeDashboardInput input)
    {
        return new TraineeDashboardDto
        {
            TraineeId = input.TraineeId,
            NutritionAdherence = await GetNutritionAdherenceAsync(
                new GetNutritionAdherenceInput { TraineeId = input.TraineeId, Date = input.Date }),
            WorkoutCompletion = await GetWorkoutCompletionAsync(
                new GetWorkoutCompletionInput { TraineeId = input.TraineeId, FromDate = input.FromDate, ToDate = input.ToDate })
        };
    }

    private async Task CheckTraineeExistsAsync(Guid traineeId)
    {
        var trainee = await _traineeRepository.FindAsync(traineeId);
        if (trainee == null)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.TraineeNotFound);
        }
    }
}
