using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Dashboards;
using CoachApp.Entites.NutritionLogs;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.Dashboards;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyDashboardAppService"/>. The adherence/
/// completion MATH is already covered by <see cref="TraineeDashboardAppServiceTests{TStartupModule}"/>
/// (both share the internal DashboardCalculator), so this suite focuses on what is unique to the
/// trainee wrapper: resolving the trainee from the current user and returning only that trainee's
/// data. Permission-level authorization (403) is not exercised (AlwaysAllowAuthorization); cross-tenant
/// isolation isn't directly testable here (single host tenant) — same-tenant cross-trainee isolation is.
/// </summary>
public abstract class MyDashboardAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyDashboardAppService _dashboard;
    private readonly IMyNutritionLogAppService _myNutritionLog;

    protected MyDashboardAppServiceTests()
    {
        _dashboard = GetRequiredService<IMyDashboardAppService>();
        _myNutritionLog = GetRequiredService<IMyNutritionLogAppService>();
    }

    [Fact]
    public async Task Should_Throw_When_Current_User_Has_No_Trainee_Profile()
    {
        // No ChangeToTrainee: the default principal is the host admin, which has no Trainee record.
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _dashboard.GetSummaryAsync(new GetMyDashboardInput
            {
                Date = new DateTime(2026, 4, 1),
                FromDate = new DateTime(2026, 4, 1),
                ToDate = new DateTime(2026, 4, 7)
            }));
    }

    [Fact]
    public async Task Adherence_Should_Reflect_Only_The_Current_Trainees_Logs()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();
        var food = await CreateFoodAsync(); // 100 kcal / serving
        var date = new DateTime(2026, 4, 1);

        using (ChangeToTrainee(trainee))
        {
            await LogFoodAsync(food.Id, date, quantity: 1m); // 100 kcal
        }

        using (ChangeToTrainee(other))
        {
            await LogFoodAsync(food.Id, date, quantity: 2m); // 200 kcal
        }

        using (ChangeToTrainee(trainee))
        {
            var mine = await _dashboard.GetNutritionAdherenceAsync(new GetMyNutritionAdherenceInput { Date = date });
            mine.HasLog.ShouldBeTrue();
            mine.ConsumedCalories.ShouldBe(100m); // only this trainee's log, not 300
        }

        using (ChangeToTrainee(other))
        {
            var theirs = await _dashboard.GetNutritionAdherenceAsync(new GetMyNutritionAdherenceInput { Date = date });
            theirs.ConsumedCalories.ShouldBe(200m);
        }
    }

    [Fact]
    public async Task Summary_Should_Compose_For_The_Current_Trainee()
    {
        var trainee = await CreateTraineeAsync();

        TraineeDashboardDto summary;
        using (ChangeToTrainee(trainee))
        {
            summary = await _dashboard.GetSummaryAsync(new GetMyDashboardInput
            {
                Date = new DateTime(2026, 4, 1),
                FromDate = new DateTime(2026, 4, 1),
                ToDate = new DateTime(2026, 4, 7)
            });
        }

        summary.TraineeId.ShouldBe(trainee.Id);
        summary.NutritionAdherence.ShouldNotBeNull();
        summary.WorkoutCompletion.ShouldNotBeNull();
    }

    private Task LogFoodAsync(Guid foodId, DateTime date, decimal quantity)
    {
        return _myNutritionLog.CreateAsync(new CreateNutritionLogDto
        {
            Date = date,
            Entries = new List<CreateNutritionLogEntryDto> { new() { FoodId = foodId, Order = 1, Quantity = quantity } }
        });
    }
}
