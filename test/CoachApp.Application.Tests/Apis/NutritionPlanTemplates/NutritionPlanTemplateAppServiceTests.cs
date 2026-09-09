using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.NutritionPlanTemplates;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.NutritionPlanTemplates;

/// <summary>
/// Integration tests for the coach-facing <see cref="INutritionPlanTemplateAppService"/>:
/// template CRUD (with macro enrichment), save-plan-as-template, and clone-onto-trainee.
/// </summary>
public abstract class NutritionPlanTemplateAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly INutritionPlanTemplateAppService _templateAppService;
    private readonly INutritionPlanAppService _planAppService;

    protected NutritionPlanTemplateAppServiceTests()
    {
        _templateAppService = GetRequiredService<INutritionPlanTemplateAppService>();
        _planAppService = GetRequiredService<INutritionPlanAppService>();
    }

    [Fact]
    public async Task Should_Create_Template_And_Compute_Macros()
    {
        // Food defaults: 100 kcal / 10p / 20c / 5f per serving.
        var food = await CreateFoodAsync("Oats");

        var result = await _templateAppService.CreateAsync(new CreateUpdateNutritionPlanTemplateDto
        {
            Name = "Bulk",
            Meals = new List<CreateUpdateNutritionTemplateMealDto>
            {
                new()
                {
                    Name = "Breakfast",
                    Order = 1,
                    Items = new List<CreateUpdateNutritionTemplateItemDto>
                    {
                        new() { FoodId = food.Id, Order = 1, Quantity = 2m }
                    }
                }
            }
        });

        result.Meals.Single().Items.Single().FoodName.ShouldBe("Oats"); // enriched
        result.Meals.Single().Items.Single().Calories.ShouldBe(200m);   // 100 * 2
        result.TotalCalories.ShouldBe(200m);
        result.TotalProteinG.ShouldBe(20m);
    }

    [Fact]
    public async Task Should_Throw_When_Referencing_Unknown_Food()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _templateAppService.CreateAsync(new CreateUpdateNutritionPlanTemplateDto
            {
                Name = "Bad",
                Meals = new List<CreateUpdateNutritionTemplateMealDto>
                {
                    new()
                    {
                        Name = "Meal",
                        Order = 1,
                        Items = new List<CreateUpdateNutritionTemplateItemDto> { new() { FoodId = Guid.NewGuid(), Order = 1, Quantity = 1m } }
                    }
                }
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.FoodsNotFound);
    }

    [Fact]
    public async Task Should_Save_Existing_Plan_As_Template()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Rice");

        var plan = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Sourced",
            IsActive = true,
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Lunch", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 3m } } }
            }
        });

        var template = await _templateAppService.SaveAsTemplateAsync(new SaveNutritionPlanAsTemplateDto
        {
            NutritionPlanId = plan.Id,
            Name = "From Plan"
        });

        template.Meals.Single().Items.Single().FoodId.ShouldBe(food.Id);
        template.Meals.Single().Items.Single().Quantity.ShouldBe(3m);
        template.TotalCalories.ShouldBe(300m); // 100 * 3
    }

    [Fact]
    public async Task Should_Clone_Template_To_Trainee_As_Inactive_Plan()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Chicken");

        var template = await _templateAppService.CreateAsync(new CreateUpdateNutritionPlanTemplateDto
        {
            Name = "Cloneable",
            Meals = new List<CreateUpdateNutritionTemplateMealDto>
            {
                new() { Name = "Dinner", Order = 1, Items = new List<CreateUpdateNutritionTemplateItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 1.5m } } }
            }
        });

        var plan = await _templateAppService.CloneToTraineeAsync(template.Id, new CloneNutritionTemplateDto
        {
            TraineeId = trainee.Id
        });

        plan.TraineeId.ShouldBe(trainee.Id);
        plan.IsActive.ShouldBeFalse();
        plan.Meals.Single().Items.Single().FoodId.ShouldBe(food.Id);
        plan.Meals.Single().Items.Single().Quantity.ShouldBe(1.5m);
        plan.TotalCalories.ShouldBe(150m); // 100 * 1.5
    }

    [Fact]
    public async Task Should_Throw_When_Cloning_To_Unknown_Trainee()
    {
        var template = await _templateAppService.CreateAsync(new CreateUpdateNutritionPlanTemplateDto { Name = "T" });

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _templateAppService.CloneToTraineeAsync(template.Id, new CloneNutritionTemplateDto { TraineeId = Guid.NewGuid() }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.TraineeNotFound);
    }

    [Fact]
    public async Task Should_Persist_Template_Macro_Targets()
    {
        var template = await _templateAppService.CreateAsync(new CreateUpdateNutritionPlanTemplateDto
        {
            Name = "Cut",
            TargetCalories = 2000m,
            TargetProteinG = 180m,
            TargetCarbsG = 150m,
            TargetFatG = 60m
        });

        template.TargetCalories.ShouldBe(2000m);
        template.TargetProteinG.ShouldBe(180m);
        template.TargetCarbsG.ShouldBe(150m);
        template.TargetFatG.ShouldBe(60m);
    }

    [Fact]
    public async Task Should_Carry_Targets_Through_SaveAsTemplate_And_Clone()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Rice");

        // A source plan carrying explicit coach-set macro targets.
        var plan = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Source",
            IsActive = true,
            TargetCalories = 2500m,
            TargetProteinG = 200m,
            TargetCarbsG = 250m,
            TargetFatG = 70m,
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Lunch", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 1m } } }
            }
        });

        // Save as a template — all four targets must survive.
        var template = await _templateAppService.SaveAsTemplateAsync(new SaveNutritionPlanAsTemplateDto
        {
            NutritionPlanId = plan.Id,
            Name = "From Plan"
        });
        template.TargetCalories.ShouldBe(2500m);
        template.TargetProteinG.ShouldBe(200m);
        template.TargetCarbsG.ShouldBe(250m);
        template.TargetFatG.ShouldBe(70m);

        // Clone back onto a trainee — all four targets must reach the new NutritionPlan.
        var cloned = await _templateAppService.CloneToTraineeAsync(template.Id, new CloneNutritionTemplateDto
        {
            TraineeId = trainee.Id
        });
        cloned.TargetCalories.ShouldBe(2500m);
        cloned.TargetProteinG.ShouldBe(200m);
        cloned.TargetCarbsG.ShouldBe(250m);
        cloned.TargetFatG.ShouldBe(70m);
    }
}
