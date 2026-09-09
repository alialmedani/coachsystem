using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis.Dashboards;
using CoachApp.Apis.NutritionPlans;
using CoachApp.Apis.WorkoutLogs;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.Today;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.Today;

/// <summary>
/// Trainee-facing "Today" view. Everything is resolved for the signed-in trainee (never a client
/// id) and composed from existing building blocks: the active workout plan's scheduled day(s), the
/// active nutrition plan, today's adherence (<see cref="DashboardCalculator"/>), and the latest log.
/// </summary>
[Authorize(CoachAppPermissions.Trainee.MyToday.Default)]
public class MyTodayAppService : MyTraineeAppServiceBase, IMyTodayAppService
{
    // Sane bounds for the requested "today" date; keeps day-range math well away from DateTime overflow.
    private static readonly DateTime MinDate = new(2000, 1, 1);
    private static readonly DateTime MaxDate = new(2100, 1, 1);

    private readonly IRepository<WorkoutPlan, Guid> _workoutPlanRepository;
    private readonly IRepository<WorkoutLog, Guid> _workoutLogRepository;
    private readonly IRepository<Exercise, Guid> _exerciseRepository;
    private readonly IRepository<NutritionPlan, Guid> _nutritionPlanRepository;
    private readonly IRepository<NutritionLog, Guid> _nutritionLogRepository;
    private readonly IRepository<Food, Guid> _foodRepository;

    public MyTodayAppService(
        IRepository<WorkoutPlan, Guid> workoutPlanRepository,
        IRepository<WorkoutLog, Guid> workoutLogRepository,
        IRepository<Exercise, Guid> exerciseRepository,
        IRepository<NutritionPlan, Guid> nutritionPlanRepository,
        IRepository<NutritionLog, Guid> nutritionLogRepository,
        IRepository<Food, Guid> foodRepository)
    {
        _workoutPlanRepository = workoutPlanRepository;
        _workoutLogRepository = workoutLogRepository;
        _exerciseRepository = exerciseRepository;
        _nutritionPlanRepository = nutritionPlanRepository;
        _nutritionLogRepository = nutritionLogRepository;
        _foodRepository = foodRepository;
    }

    public virtual async Task<MyTodayDto> GetAsync(GetMyTodayInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        // Clamp to a sane window: guards the day-range math (and the downstream calculator) against
        // extreme dates like DateTime.MaxValue that would overflow AddDays and surface as a 500.
        var requested = (input.Date ?? DateTime.UtcNow).Date;
        var date = requested < MinDate ? MinDate : (requested > MaxDate ? MaxDate : requested);
        var dayStart = date;
        var dayEnd = date.AddDays(1);

        var result = new MyTodayDto
        {
            Date = date,
            DayOfWeek = date.DayOfWeek
        };

        await FillWorkoutAsync(result, traineeId, date, dayStart, dayEnd);
        await FillNutritionAsync(result, traineeId, date, dayStart, dayEnd);

        return result;
    }

    private async Task FillWorkoutAsync(MyTodayDto result, Guid traineeId, DateTime date, DateTime dayStart, DateTime dayEnd)
    {
        var activePlan = await _workoutPlanRepository.FirstOrDefaultAsync(p => p.TraineeId == traineeId && p.IsActive);
        result.HasActiveWorkoutPlan = activePlan != null;

        if (activePlan != null)
        {
            var plan = await _workoutPlanRepository.GetAsync(activePlan.Id, includeDetails: true);
            result.WorkoutPlanId = plan.Id;

            var todaysDays = plan.Days
                .Where(d => d.ScheduledDay == date.DayOfWeek)
                .OrderBy(d => d.Order)
                .ToList();

            result.ScheduledWorkoutDays = ObjectMapper.Map<List<WorkoutDay>, List<WorkoutDayDto>>(todaysDays);
            await EnrichExerciseNamesAsync(result.ScheduledWorkoutDays);
        }

        // A rest day only applies when the trainee HAS an active plan but nothing is scheduled today;
        // with no active plan there is no program at all (HasActiveWorkoutPlan already conveys that).
        result.IsRestDay = result.HasActiveWorkoutPlan && result.ScheduledWorkoutDays.Count == 0;

        // Most recent workout log for the date (multiple logs per day are allowed — D5).
        var logQuery = (await _workoutLogRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == traineeId && x.Date >= dayStart && x.Date < dayEnd);
        var latest = await AsyncExecuter.FirstOrDefaultAsync(logQuery.OrderByDescending(x => x.CreationTime));

        result.AlreadyLoggedWorkoutToday = latest != null;
        if (latest != null)
        {
            result.LatestWorkoutLogId = latest.Id;

            var detailQuery = await _workoutLogRepository.WithDetailsAsync(x => x.Entries);
            var latestWithDetails = await AsyncExecuter.FirstOrDefaultAsync(detailQuery.Where(x => x.Id == latest.Id));
            if (latestWithDetails != null)
            {
                var logDto = ObjectMapper.Map<WorkoutLog, WorkoutLogDto>(latestWithDetails);
                await WorkoutLogEnricher.EnrichAsync(logDto, _exerciseRepository);
                result.LatestWorkoutLog = logDto;
            }
        }
    }

    private async Task FillNutritionAsync(MyTodayDto result, Guid traineeId, DateTime date, DateTime dayStart, DateTime dayEnd)
    {
        var activePlan = await _nutritionPlanRepository.FirstOrDefaultAsync(p => p.TraineeId == traineeId && p.IsActive);
        result.HasActiveNutritionPlan = activePlan != null;

        if (activePlan != null)
        {
            var plan = await _nutritionPlanRepository.GetAsync(activePlan.Id, includeDetails: true);
            var planDto = ObjectMapper.Map<NutritionPlan, NutritionPlanDto>(plan);
            await NutritionPlanEnricher.EnrichAsync(planDto, _foodRepository);
            result.NutritionPlan = planDto;
        }

        result.NutritionAdherence = await DashboardCalculator.ComputeNutritionAdherenceAsync(
            traineeId, date,
            _nutritionLogRepository, _nutritionPlanRepository, _foodRepository,
            ObjectMapper, AsyncExecuter);

        result.AlreadyLoggedNutritionToday = result.NutritionAdherence.HasLog;
    }

    private async Task EnrichExerciseNamesAsync(List<WorkoutDayDto> days)
    {
        var ids = days.SelectMany(d => d.Exercises).Select(e => e.ExerciseId).Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var names = (await _exerciseRepository.GetListAsync(x => ids.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => x.Name);

        foreach (var day in days)
        {
            foreach (var ex in day.Exercises)
            {
                if (names.TryGetValue(ex.ExerciseId, out var name))
                {
                    ex.ExerciseName = name;
                }
            }
        }
    }
}
