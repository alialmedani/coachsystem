using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Dashboards;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.Dashboards;

/// <summary>
/// Integration tests for the coach-facing <see cref="ITraineeDashboardAppService"/>: nutrition
/// adherence (consumed-vs-active-plan) and workout completion (logged-vs-planned). Logs are
/// seeded through the repositories (they are normally trainee-created via the current user).
/// The trainee-facing MyDashboard service shares the same DashboardCalculator, so this covers
/// the calculation for both.
/// </summary>
public abstract class TraineeDashboardAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ITraineeDashboardAppService _dashboard;
    private readonly INutritionPlanAppService _nutritionPlanAppService;
    private readonly IWorkoutPlanAppService _workoutPlanAppService;

    protected TraineeDashboardAppServiceTests()
    {
        _dashboard = GetRequiredService<ITraineeDashboardAppService>();
        _nutritionPlanAppService = GetRequiredService<INutritionPlanAppService>();
        _workoutPlanAppService = GetRequiredService<IWorkoutPlanAppService>();
    }

    [Fact]
    public async Task Nutrition_Adherence_Should_Compare_Consumed_Vs_Active_Plan()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Oats"); // 100 kcal / 10p / 20c / 5f per serving
        var date = new DateTime(2026, 3, 2);

        // Active plan target: quantity 2 => 200 kcal / 20p / 40c / 10f.
        await _nutritionPlanAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Active",
            IsActive = true,
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Breakfast", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 2m } } }
            }
        });

        // Consumed: one log with quantity 1 => 100 kcal.
        await SeedNutritionLogAsync(trainee.Id, date, food.Id, quantity: 1m);

        var result = await _dashboard.GetNutritionAdherenceAsync(new GetNutritionAdherenceInput
        {
            TraineeId = trainee.Id,
            Date = date
        });

        result.HasActivePlan.ShouldBeTrue();
        result.HasLog.ShouldBeTrue();
        result.ConsumedCalories.ShouldBe(100m);
        result.TargetCalories.ShouldBe(200m);
        result.CaloriesPercent.ShouldBe(50m);
        result.OverallPercent.ShouldBe(50m);
    }

    [Fact]
    public async Task Nutrition_Adherence_Should_Sum_Multiple_Logs_On_Same_Day()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync();
        var date = new DateTime(2026, 3, 2);

        await SeedNutritionLogAsync(trainee.Id, date, food.Id, quantity: 1m);
        await SeedNutritionLogAsync(trainee.Id, date, food.Id, quantity: 2m);

        var result = await _dashboard.GetNutritionAdherenceAsync(new GetNutritionAdherenceInput
        {
            TraineeId = trainee.Id,
            Date = date
        });

        result.HasLog.ShouldBeTrue();
        result.ConsumedCalories.ShouldBe(300m); // (1 + 2) * 100
        result.HasActivePlan.ShouldBeFalse();     // no plan => percents null
        result.CaloriesPercent.ShouldBeNull();
    }

    [Fact]
    public async Task Nutrition_Adherence_Should_Handle_No_Plan_And_No_Log()
    {
        var trainee = await CreateTraineeAsync();

        var result = await _dashboard.GetNutritionAdherenceAsync(new GetNutritionAdherenceInput
        {
            TraineeId = trainee.Id,
            Date = new DateTime(2026, 3, 2)
        });

        result.HasActivePlan.ShouldBeFalse();
        result.HasLog.ShouldBeFalse();
        result.ConsumedCalories.ShouldBe(0m);
        result.OverallPercent.ShouldBeNull();
    }

    [Fact]
    public async Task Workout_Completion_Should_Compare_Logged_Vs_Planned()
    {
        var trainee = await CreateTraineeAsync();

        // Active plan with 2 scheduled weekdays => planned 2 per week.
        await _workoutPlanAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Active",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Day 1", Order = 1, ScheduledDay = DayOfWeek.Monday },
                new() { Name = "Day 2", Order = 2, ScheduledDay = DayOfWeek.Wednesday }
            }
        });

        var from = new DateTime(2026, 3, 2);
        var to = new DateTime(2026, 3, 8); // inclusive 7 days => 1 week
        await SeedWorkoutLogAsync(trainee.Id, from);
        await SeedWorkoutLogAsync(trainee.Id, from.AddDays(1));
        await SeedWorkoutLogAsync(trainee.Id, from.AddDays(2));

        var result = await _dashboard.GetWorkoutCompletionAsync(new GetWorkoutCompletionInput
        {
            TraineeId = trainee.Id,
            FromDate = from,
            ToDate = to
        });

        result.HasActivePlan.ShouldBeTrue();
        result.PlannedPerWeek.ShouldBe(2);
        result.Weeks.ShouldBe(1);
        result.PlannedSessions.ShouldBe(2);
        result.CompletedSessions.ShouldBe(3);
        result.CompletionPercent.ShouldBe(150m); // not capped
    }

    [Fact]
    public async Task Workout_Completion_Should_Handle_No_Active_Plan()
    {
        var trainee = await CreateTraineeAsync();
        var from = new DateTime(2026, 3, 2);
        await SeedWorkoutLogAsync(trainee.Id, from);

        var result = await _dashboard.GetWorkoutCompletionAsync(new GetWorkoutCompletionInput
        {
            TraineeId = trainee.Id,
            FromDate = from,
            ToDate = from.AddDays(6)
        });

        result.HasActivePlan.ShouldBeFalse();
        result.CompletedSessions.ShouldBe(1);
        result.PlannedSessions.ShouldBeNull();
        result.CompletionPercent.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Throw_For_Unknown_Trainee()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _dashboard.GetNutritionAdherenceAsync(new GetNutritionAdherenceInput
            {
                TraineeId = Guid.NewGuid(),
                Date = new DateTime(2026, 3, 2)
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.TraineeNotFound);
    }

    [Fact]
    public async Task Workout_Completion_Should_Ignore_Unscheduled_Days()
    {
        var trainee = await CreateTraineeAsync();

        // One scheduled day (Monday) + one day with no ScheduledDay (reference/unscheduled).
        await _workoutPlanAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Active",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Scheduled", Order = 1, ScheduledDay = DayOfWeek.Monday },
                new() { Name = "Unscheduled", Order = 2 }
            }
        });

        var from = new DateTime(2026, 3, 2);
        var result = await _dashboard.GetWorkoutCompletionAsync(new GetWorkoutCompletionInput
        {
            TraineeId = trainee.Id,
            FromDate = from,
            ToDate = from.AddDays(6)
        });

        result.PlannedPerWeek.ShouldBe(1);   // the unscheduled day is not counted
        result.Weeks.ShouldBe(1);
        result.PlannedSessions.ShouldBe(1);
    }

    [Fact]
    public async Task Workout_Completion_Should_Count_Duplicate_Scheduled_Weekdays_Once()
    {
        var trainee = await CreateTraineeAsync();

        // F2/PD9 forbids creating duplicate scheduled weekdays through the app service, so seed a
        // legacy-shaped plan directly via the repository to verify the completion calculator still
        // defensively counts a weekday once.
        await WithUnitOfWorkAsync(async () =>
        {
            var repo = GetRequiredService<IRepository<WorkoutPlan, Guid>>();
            var plan = WorkoutPlan.Create(Guid.NewGuid(), trainee.Id, "Active");
            plan.AddDay(Guid.NewGuid(), "Push", 1, DayOfWeek.Monday);
            plan.AddDay(Guid.NewGuid(), "Pull", 2, DayOfWeek.Monday);
            plan.Activate();
            await repo.InsertAsync(plan, autoSave: true);
        });

        var from = new DateTime(2026, 3, 2);
        var result = await _dashboard.GetWorkoutCompletionAsync(new GetWorkoutCompletionInput
        {
            TraineeId = trainee.Id,
            FromDate = from,
            ToDate = from.AddDays(6)
        });

        result.PlannedPerWeek.ShouldBe(1);
        result.PlannedSessions.ShouldBe(1);
    }

    [Fact]
    public async Task Workout_Completion_Should_Count_Distinct_Scheduled_Weekdays()
    {
        var trainee = await CreateTraineeAsync();

        // Three distinct scheduled weekdays => planned 3 per week.
        await _workoutPlanAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Active",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Day 1", Order = 1, ScheduledDay = DayOfWeek.Monday },
                new() { Name = "Day 2", Order = 2, ScheduledDay = DayOfWeek.Wednesday },
                new() { Name = "Day 3", Order = 3, ScheduledDay = DayOfWeek.Friday }
            }
        });

        var from = new DateTime(2026, 3, 2); // Monday
        var to = from.AddDays(6);            // inclusive week
        await SeedWorkoutLogAsync(trainee.Id, from);
        await SeedWorkoutLogAsync(trainee.Id, from.AddDays(2));
        await SeedWorkoutLogAsync(trainee.Id, from.AddDays(4));

        var result = await _dashboard.GetWorkoutCompletionAsync(new GetWorkoutCompletionInput
        {
            TraineeId = trainee.Id,
            FromDate = from,
            ToDate = to
        });

        result.PlannedPerWeek.ShouldBe(3);
        result.Weeks.ShouldBe(1);
        result.PlannedSessions.ShouldBe(3);
        result.CompletedSessions.ShouldBe(3);
        result.CompletionPercent.ShouldBe(100m);
    }

    [Fact]
    public async Task Summary_Should_Include_Weekly_Nutrition_Adherence_Range()
    {
        // F5/PD5: range adherence averages daily consumption over the days actually logged.
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Oats"); // 100 kcal per serving
        var from = new DateTime(2026, 3, 2);
        var to = from.AddDays(6); // 7-day window

        // Active plan target: quantity 2 => 200 kcal/day.
        await _nutritionPlanAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Active",
            IsActive = true,
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Breakfast", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 2m } } }
            }
        });

        // Logged on two distinct days: 100 kcal each => total 200, over 2 logged days.
        await SeedNutritionLogAsync(trainee.Id, from, food.Id, quantity: 1m);
        await SeedNutritionLogAsync(trainee.Id, from.AddDays(2), food.Id, quantity: 1m);

        var summary = await _dashboard.GetSummaryAsync(new GetTraineeDashboardInput
        {
            TraineeId = trainee.Id,
            Date = from,
            FromDate = from,
            ToDate = to
        });

        var range = summary.NutritionAdherenceRange.ShouldNotBeNull();
        range.DaysInRange.ShouldBe(7);
        range.DaysLogged.ShouldBe(2);
        range.HasActivePlan.ShouldBeTrue();
        range.ConsumedCaloriesTotal.ShouldBe(200m);
        range.TargetCaloriesPerDay.ShouldBe(200m);
        range.AverageCaloriesPercent.ShouldBe(50m); // 200 / (200 * 2 days) * 100
    }

    private Task SeedNutritionLogAsync(Guid traineeId, DateTime date, Guid foodId, decimal quantity)
    {
        var repo = GetRequiredService<IRepository<NutritionLog, Guid>>();
        return WithUnitOfWorkAsync(async () =>
        {
            var log = NutritionLog.Create(Guid.NewGuid(), traineeId, date);
            log.AddEntry(Guid.NewGuid(), foodId, 1, quantity);
            await repo.InsertAsync(log, autoSave: true);
        });
    }

    private Task SeedWorkoutLogAsync(Guid traineeId, DateTime date)
    {
        var repo = GetRequiredService<IRepository<WorkoutLog, Guid>>();
        return WithUnitOfWorkAsync(async () =>
        {
            var log = WorkoutLog.Create(Guid.NewGuid(), traineeId, date);
            await repo.InsertAsync(log, autoSave: true);
        });
    }
}
