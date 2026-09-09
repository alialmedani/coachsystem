using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.Today;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.Today;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyTodayAppService"/>: today's scheduled
/// workout day(s) resolved from the active plan by weekday, rest days, the active nutrition plan +
/// adherence, and the already-logged-today status. All reads run impersonating the trainee.
/// </summary>
public abstract class MyTodayAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyTodayAppService _today;
    private readonly IWorkoutPlanAppService _workoutPlanAppService;
    private readonly INutritionPlanAppService _nutritionPlanAppService;
    private readonly IMyWorkoutLogAppService _myLog;

    protected MyTodayAppServiceTests()
    {
        _today = GetRequiredService<IMyTodayAppService>();
        _workoutPlanAppService = GetRequiredService<IWorkoutPlanAppService>();
        _nutritionPlanAppService = GetRequiredService<INutritionPlanAppService>();
        _myLog = GetRequiredService<IMyWorkoutLogAppService>();
    }

    private static readonly DateTime SampleDate = new(2026, 3, 2);
    private static DayOfWeek Today => SampleDate.DayOfWeek;
    private static DayOfWeek NotToday => (DayOfWeek)(((int)SampleDate.DayOfWeek + 1) % 7);

    [Fact]
    public async Task Should_Return_Todays_Scheduled_Workout_Day_With_Exercises()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync("Squat");

        var plan = await _workoutPlanAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Weekly",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new()
                {
                    Name = "Today's Day",
                    Order = 1,
                    ScheduledDay = Today,
                    Exercises = new List<CreateUpdateWorkoutExerciseDto>
                    {
                        new() { ExerciseId = ex.Id, Order = 1, Sets = 5, Reps = "5", WeightKg = 100m }
                    }
                },
                new() { Name = "Another Day", Order = 2, ScheduledDay = NotToday }
            }
        });

        MyTodayDto result;
        using (ChangeToTrainee(trainee))
        {
            result = await _today.GetAsync(new GetMyTodayInput { Date = SampleDate });
        }

        result.Date.ShouldBe(SampleDate);
        result.DayOfWeek.ShouldBe(Today);
        result.HasActiveWorkoutPlan.ShouldBeTrue();
        result.WorkoutPlanId.ShouldBe(plan.Id);
        result.IsRestDay.ShouldBeFalse();
        result.ScheduledWorkoutDays.Count.ShouldBe(1);

        var day = result.ScheduledWorkoutDays.Single();
        day.Name.ShouldBe("Today's Day");
        day.Exercises.Single().ExerciseName.ShouldBe("Squat"); // enriched
    }

    [Fact]
    public async Task Should_Report_Rest_Day_When_Nothing_Scheduled_Today()
    {
        var trainee = await CreateTraineeAsync();

        await _workoutPlanAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Weekly",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Other Day", Order = 1, ScheduledDay = NotToday }
            }
        });

        MyTodayDto result;
        using (ChangeToTrainee(trainee))
        {
            result = await _today.GetAsync(new GetMyTodayInput { Date = SampleDate });
        }

        result.HasActiveWorkoutPlan.ShouldBeTrue();
        result.IsRestDay.ShouldBeTrue();
        result.ScheduledWorkoutDays.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Surface_Active_Nutrition_Plan_And_Adherence()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Oats");

        await _nutritionPlanAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Cut",
            IsActive = true,
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Breakfast", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 2m } } }
            }
        });

        MyTodayDto result;
        using (ChangeToTrainee(trainee))
        {
            result = await _today.GetAsync(new GetMyTodayInput { Date = SampleDate });
        }

        result.HasActiveNutritionPlan.ShouldBeTrue();
        result.NutritionPlan.ShouldNotBeNull();
        result.NutritionPlan!.Meals.ShouldNotBeEmpty();
        result.NutritionAdherence.ShouldNotBeNull();
        result.NutritionAdherence.HasActivePlan.ShouldBeTrue();
        result.AlreadyLoggedNutritionToday.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Reflect_Already_Logged_Workout_Today()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync();

        var plan = await _workoutPlanAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Weekly",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new()
                {
                    Name = "Today's Day",
                    Order = 1,
                    ScheduledDay = Today,
                    Exercises = new List<CreateUpdateWorkoutExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 3 } }
                }
            }
        });

        MyTodayDto result;
        using (ChangeToTrainee(trainee))
        {
            await _myLog.CreateFromDayAsync(new CreateWorkoutLogFromDayDto
            {
                WorkoutDayId = plan.Days.Single().Id,
                Date = SampleDate
            });

            result = await _today.GetAsync(new GetMyTodayInput { Date = SampleDate });
        }

        result.AlreadyLoggedWorkoutToday.ShouldBeTrue();
        result.LatestWorkoutLogId.ShouldNotBeNull();
        result.LatestWorkoutLog.ShouldNotBeNull();
        result.LatestWorkoutLog!.Entries.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_Default_Date_To_UtcToday_When_Omitted()
    {
        var trainee = await CreateTraineeAsync();

        MyTodayDto result;
        using (ChangeToTrainee(trainee))
        {
            result = await _today.GetAsync(new GetMyTodayInput());
        }

        result.Date.ShouldBe(DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task Should_Handle_No_Active_Plans_Gracefully()
    {
        var trainee = await CreateTraineeAsync();

        MyTodayDto result;
        using (ChangeToTrainee(trainee))
        {
            result = await _today.GetAsync(new GetMyTodayInput { Date = SampleDate });
        }

        result.HasActiveWorkoutPlan.ShouldBeFalse();
        result.WorkoutPlanId.ShouldBeNull();
        result.IsRestDay.ShouldBeTrue();
        result.HasActiveNutritionPlan.ShouldBeFalse();
        result.NutritionPlan.ShouldBeNull();
        result.NutritionAdherence.ShouldNotBeNull();
        result.NutritionAdherence.HasActivePlan.ShouldBeFalse();
        result.AlreadyLoggedWorkoutToday.ShouldBeFalse();
    }
}
