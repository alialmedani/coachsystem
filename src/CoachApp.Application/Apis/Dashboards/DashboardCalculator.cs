using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis.NutritionLogs;
using CoachApp.Apis.NutritionPlans;
using CoachApp.Entites.Dashboards;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.ObjectMapping;

namespace CoachApp.Apis.Dashboards;

/// <summary>
/// Read-only adherence/completion math shared by the coach (<see cref="TraineeDashboardAppService"/>)
/// and trainee (<see cref="MyDashboardAppService"/>) dashboards. It never trusts a caller for the
/// trainee id — the caller resolves that first (coach from input, trainee from the current user) —
/// and it reuses the existing nutrition enrichers so per-serving macro math lives in exactly one place.
/// </summary>
internal static class DashboardCalculator
{
    public static async Task<NutritionAdherenceDto> ComputeNutritionAdherenceAsync(
        Guid traineeId,
        DateTime date,
        IRepository<NutritionLog, Guid> nutritionLogRepository,
        IRepository<NutritionPlan, Guid> nutritionPlanRepository,
        IRepository<Food, Guid> foodRepository,
        IObjectMapper objectMapper,
        IAsyncQueryableExecuter asyncExecuter)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        // Consumed: sum the totals of every log on this day (multiple logs are summed).
        var logQuery = (await nutritionLogRepository.WithDetailsAsync(x => x.Entries))
            .Where(x => x.TraineeId == traineeId && x.Date >= dayStart && x.Date < dayEnd);
        var logs = await asyncExecuter.ToListAsync(logQuery);

        decimal consumedCalories = 0, consumedProtein = 0, consumedCarbs = 0, consumedFat = 0;
        foreach (var log in logs)
        {
            var logDto = objectMapper.Map<NutritionLog, NutritionLogDto>(log);
            await NutritionLogEnricher.EnrichAsync(logDto, foodRepository);
            consumedCalories += logDto.TotalCalories;
            consumedProtein += logDto.TotalProteinG;
            consumedCarbs += logDto.TotalCarbsG;
            consumedFat += logDto.TotalFatG;
        }

        var dto = new NutritionAdherenceDto
        {
            Date = dayStart,
            HasLog = logs.Count > 0,
            ConsumedCalories = consumedCalories,
            ConsumedProteinG = consumedProtein,
            ConsumedCarbsG = consumedCarbs,
            ConsumedFatG = consumedFat
        };

        // Target: the trainee's active plan (at most one), with its computed totals.
        var planQuery = (await nutritionPlanRepository.WithDetailsAsync())
            .Where(p => p.TraineeId == traineeId && p.IsActive);
        var activePlan = await asyncExecuter.FirstOrDefaultAsync(planQuery);

        dto.HasActivePlan = activePlan != null;
        if (activePlan != null)
        {
            var planDto = objectMapper.Map<NutritionPlan, NutritionPlanDto>(activePlan);
            await NutritionPlanEnricher.EnrichAsync(planDto, foodRepository);

            dto.TargetCalories = planDto.TotalCalories;
            dto.TargetProteinG = planDto.TotalProteinG;
            dto.TargetCarbsG = planDto.TotalCarbsG;
            dto.TargetFatG = planDto.TotalFatG;

            dto.CaloriesPercent = Percent(consumedCalories, planDto.TotalCalories);
            dto.ProteinPercent = Percent(consumedProtein, planDto.TotalProteinG);
            dto.CarbsPercent = Percent(consumedCarbs, planDto.TotalCarbsG);
            dto.FatPercent = Percent(consumedFat, planDto.TotalFatG);
            dto.OverallPercent = dto.CaloriesPercent;
        }

        return dto;
    }

    public static async Task<WorkoutCompletionDto> ComputeWorkoutCompletionAsync(
        Guid traineeId,
        DateTime fromDate,
        DateTime toDate,
        IRepository<WorkoutLog, Guid> workoutLogRepository,
        IRepository<WorkoutPlan, Guid> workoutPlanRepository,
        IAsyncQueryableExecuter asyncExecuter)
    {
        var from = fromDate.Date;
        var to = toDate.Date;
        var rangeEnd = to.AddDays(1);

        var logQuery = (await workoutLogRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == traineeId && x.Date >= from && x.Date < rangeEnd);
        var logDates = await asyncExecuter.ToListAsync(logQuery.Select(x => x.Date));
        var completed = logDates.Select(d => d.Date).Distinct().Count();

        var inclusiveDays = Math.Max(1, (to - from).Days + 1);
        var weeks = Math.Max(1, (int)Math.Ceiling(inclusiveDays / 7.0));

        var dto = new WorkoutCompletionDto
        {
            FromDate = from,
            ToDate = to,
            Weeks = weeks,
            CompletedSessions = completed
        };

        var planQuery = (await workoutPlanRepository.WithDetailsAsync())
            .Where(p => p.TraineeId == traineeId && p.IsActive);
        var activePlan = await asyncExecuter.FirstOrDefaultAsync(planQuery);

        dto.HasActivePlan = activePlan != null;
        if (activePlan != null)
        {
            var plannedPerWeek = activePlan.Days.Count;
            var plannedSessions = plannedPerWeek * weeks;
            dto.PlannedPerWeek = plannedPerWeek;
            dto.PlannedSessions = plannedSessions;
            dto.CompletionPercent = plannedSessions > 0
                ? Math.Round((decimal)completed / plannedSessions * 100m, 1)
                : null;
        }

        return dto;
    }

    private static decimal? Percent(decimal consumed, decimal target)
        => target <= 0 ? null : Math.Round(consumed / target * 100m, 1);
}
