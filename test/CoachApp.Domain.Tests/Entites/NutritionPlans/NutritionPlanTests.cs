using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.NutritionPlans;

/// <summary>
/// Pure domain tests for the <see cref="NutritionPlan"/> aggregate: the <c>Create</c>
/// factory and the behaviour methods that build the Plan → Meal → Item graph.
/// </summary>
public class NutritionPlanTests
{
    [Fact]
    public void Should_Create_Plan_Inactive_With_No_Meals()
    {
        var id = Guid.NewGuid();
        var traineeId = Guid.NewGuid();

        var plan = NutritionPlan.Create(id, traineeId, "Cutting Plan", "1800 kcal");

        plan.Id.ShouldBe(id);
        plan.TraineeId.ShouldBe(traineeId);
        plan.Name.ShouldBe("Cutting Plan");
        plan.Description.ShouldBe("1800 kcal");
        plan.IsActive.ShouldBeFalse();
        plan.Meals.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_NullOrWhitespace(string? name)
    {
        Should.Throw<ArgumentException>(() =>
            NutritionPlan.Create(Guid.NewGuid(), Guid.NewGuid(), name!));
    }

    [Fact]
    public void Should_Build_Full_Meal_And_Item_Graph()
    {
        var plan = NutritionPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");

        var mealId = Guid.NewGuid();
        var meal = plan.AddMeal(mealId, "Breakfast", 1);

        meal.Id.ShouldBe(mealId);
        meal.NutritionPlanId.ShouldBe(plan.Id);
        meal.Name.ShouldBe("Breakfast");
        meal.Order.ShouldBe(1);

        var itemId = Guid.NewGuid();
        var foodId = Guid.NewGuid();
        var item = meal.AddItem(itemId, foodId, 1, 2.5m);

        item.Id.ShouldBe(itemId);
        item.MealId.ShouldBe(meal.Id);
        item.FoodId.ShouldBe(foodId);
        item.Order.ShouldBe(1);
        item.Quantity.ShouldBe(2.5m);

        plan.Meals.Count.ShouldBe(1);
        meal.Items.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Clear_All_Meals()
    {
        var plan = NutritionPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");
        plan.AddMeal(Guid.NewGuid(), "Breakfast", 1);
        plan.AddMeal(Guid.NewGuid(), "Lunch", 2);

        plan.ClearMeals();

        plan.Meals.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Meal_Name_Is_NullOrWhitespace(string? mealName)
    {
        var plan = NutritionPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");

        Should.Throw<ArgumentException>(() => plan.AddMeal(Guid.NewGuid(), mealName!, 1));
    }
}
