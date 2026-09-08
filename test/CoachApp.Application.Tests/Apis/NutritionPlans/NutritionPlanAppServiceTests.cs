using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.NutritionPlans;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.NutritionPlans;

/// <summary>
/// Integration tests for the coach-facing <see cref="INutritionPlanAppService"/>: the
/// nested Plan → Meal → Item graph, the computed per-item and plan-total macros, the
/// cross-aggregate food/trainee existence checks, and the one-active-plan rule.
/// </summary>
public abstract class NutritionPlanAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly INutritionPlanAppService _planAppService;

    protected NutritionPlanAppServiceTests()
    {
        _planAppService = GetRequiredService<INutritionPlanAppService>();
    }

    [Fact]
    public async Task Should_Create_Plan_And_Compute_Item_And_Total_Macros()
    {
        var trainee = await CreateTraineeAsync();

        // Per-serving values.
        var chicken = await CreateFoodAsync("Chicken", calories: 165m, proteinG: 31m, carbsG: 0m, fatG: 3.6m);
        var rice = await CreateFoodAsync("Rice", calories: 130m, proteinG: 2.7m, carbsG: 28m, fatG: 0.3m);

        var result = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Lunch Plan",
            Meals = new List<CreateUpdateMealDto>
            {
                new()
                {
                    Name = "Lunch",
                    Order = 1,
                    Items = new List<CreateUpdateMealItemDto>
                    {
                        new() { FoodId = chicken.Id, Order = 1, Quantity = 2m },  // 2 servings
                        new() { FoodId = rice.Id, Order = 2, Quantity = 1.5m }     // 1.5 servings
                    }
                }
            }
        });

        var meal = result.Meals.Single();
        var chickenItem = meal.Items.Single(i => i.FoodId == chicken.Id);
        var riceItem = meal.Items.Single(i => i.FoodId == rice.Id);

        // Per-item = per-serving * quantity.
        chickenItem.Calories.ShouldBe(330m);   // 165 * 2
        chickenItem.ProteinG.ShouldBe(62m);     // 31 * 2
        chickenItem.FoodName.ShouldBe("Chicken");

        riceItem.Calories.ShouldBe(195m);        // 130 * 1.5
        riceItem.CarbsG.ShouldBe(42m);           // 28 * 1.5

        // Plan totals = sum of item values.
        result.TotalCalories.ShouldBe(525m);     // 330 + 195
        result.TotalProteinG.ShouldBe(66.05m);   // 62 + 4.05
        result.TotalCarbsG.ShouldBe(42m);        // 0 + 42
        result.TotalFatG.ShouldBe(7.65m);        // 7.2 + 0.45
    }

    [Fact]
    public async Task Should_Throw_When_Trainee_Does_Not_Exist()
    {
        await Should.ThrowAsync<BusinessException>(() =>
            _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
            {
                TraineeId = Guid.NewGuid(),
                Name = "Orphan"
            }));
    }

    [Fact]
    public async Task Should_Throw_When_Referencing_Unknown_Food()
    {
        var trainee = await CreateTraineeAsync();

        await Should.ThrowAsync<BusinessException>(() =>
            _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
            {
                TraineeId = trainee.Id,
                Name = "Bad Plan",
                Meals = new List<CreateUpdateMealDto>
                {
                    new()
                    {
                        Name = "Meal",
                        Order = 1,
                        Items = new List<CreateUpdateMealItemDto>
                        {
                            new() { FoodId = Guid.NewGuid(), Order = 1, Quantity = 1m }
                        }
                    }
                }
            }));
    }

    [Fact]
    public async Task Should_Deactivate_Other_Plans_When_A_Plan_Is_Set_Active()
    {
        var trainee = await CreateTraineeAsync();

        var plan1 = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan 1",
            IsActive = true
        });
        plan1.IsActive.ShouldBeTrue();

        var plan2 = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan 2",
            IsActive = false
        });

        var activated = await _planAppService.SetActiveAsync(plan2.Id);
        activated.IsActive.ShouldBeTrue();

        (await _planAppService.GetAsync(plan1.Id)).IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Replace_Meals_On_Update()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        var created = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan",
            Meals = new List<CreateUpdateMealDto> { new() { Name = "Breakfast", Order = 1 } }
        });
        created.Meals.Count.ShouldBe(1);

        var updated = await _planAppService.UpdateAsync(created.Id, new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan",
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Breakfast", Order = 1 },
                new()
                {
                    Name = "Lunch",
                    Order = 2,
                    Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 1m } }
                }
            }
        });

        updated.Meals.Count.ShouldBe(2);
    }
}
