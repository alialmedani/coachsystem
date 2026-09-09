using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.NutritionPlans;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.NutritionPlans;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyNutritionPlanAppService"/> (read-only).
/// Plans are seeded through the coach service, then read impersonating the trainee via ChangeToTrainee.
/// Permission-level authorization (403) is not exercised (AlwaysAllowAuthorization); these assert the
/// real enforcement: trainee resolved from the current user + per-plan ownership. Cross-tenant isolation
/// isn't directly testable here (single host tenant) — same-tenant cross-trainee isolation is.
/// </summary>
public abstract class MyNutritionPlanAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyNutritionPlanAppService _myPlan;
    private readonly INutritionPlanAppService _coachPlan;

    protected MyNutritionPlanAppServiceTests()
    {
        _myPlan = GetRequiredService<IMyNutritionPlanAppService>();
        _coachPlan = GetRequiredService<INutritionPlanAppService>();
    }

    [Fact]
    public async Task GetActive_Should_Return_The_Active_Plan_With_Computed_Macros()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Chicken"); // 100 kcal / serving

        await _coachPlan.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Cut",
            IsActive = true,
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Lunch", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 2m } } }
            }
        });

        NutritionPlanDto? active;
        using (ChangeToTrainee(trainee))
        {
            active = await _myPlan.GetActiveAsync();
        }

        active.ShouldNotBeNull();
        active!.Name.ShouldBe("Cut");
        active.Meals.ShouldNotBeEmpty();
        active.TotalCalories.ShouldBe(200m); // 100 * 2, enriched
    }

    [Fact]
    public async Task GetActive_Should_Return_Null_When_No_Active_Plan()
    {
        var trainee = await CreateTraineeAsync();
        await _coachPlan.CreateAsync(new CreateUpdateNutritionPlanDto { TraineeId = trainee.Id, Name = "Draft", IsActive = false });

        using (ChangeToTrainee(trainee))
        {
            (await _myPlan.GetActiveAsync()).ShouldBeNull();
        }
    }

    [Fact]
    public async Task GetList_Should_Return_Only_Own_Plans()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        await _coachPlan.CreateAsync(new CreateUpdateNutritionPlanDto { TraineeId = trainee.Id, Name = "Mine" });
        await _coachPlan.CreateAsync(new CreateUpdateNutritionPlanDto { TraineeId = other.Id, Name = "Theirs" });

        using (ChangeToTrainee(trainee))
        {
            var list = await _myPlan.GetListAsync();
            list.Count.ShouldBe(1);
            list.Single().TraineeId.ShouldBe(trainee.Id);
        }
    }

    [Fact]
    public async Task Get_Should_Throw_For_A_Plan_Owned_By_Another_Trainee()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        var plan = await _coachPlan.CreateAsync(new CreateUpdateNutritionPlanDto { TraineeId = trainee.Id, Name = "Mine" });

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _myPlan.GetAsync(plan.Id));
        }
    }
}
