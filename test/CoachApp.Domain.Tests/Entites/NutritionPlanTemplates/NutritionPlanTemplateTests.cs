using System;
using CoachApp.Entites.NutritionPlans;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.NutritionPlanTemplates;

/// <summary>
/// Pure domain tests for the <see cref="NutritionPlanTemplate"/> aggregate: the <c>Create</c>
/// factory (no trainee / no active state) and the Meal → Item build methods.
/// </summary>
public class NutritionPlanTemplateTests
{
    [Fact]
    public void Should_Create_Template_With_No_Meals()
    {
        var id = Guid.NewGuid();

        var template = NutritionPlanTemplate.Create(id, "Cutting 2000kcal", "high protein");

        template.Id.ShouldBe(id);
        template.Name.ShouldBe("Cutting 2000kcal");
        template.Description.ShouldBe("high protein");
        template.Meals.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_NullOrWhitespace(string? name)
    {
        Should.Throw<ArgumentException>(() => NutritionPlanTemplate.Create(Guid.NewGuid(), name!));
    }

    [Fact]
    public void Should_Build_Full_Meal_And_Item_Graph()
    {
        var template = NutritionPlanTemplate.Create(Guid.NewGuid(), "Template");
        var meal = template.AddMeal(Guid.NewGuid(), "Breakfast", 1);

        var itemId = Guid.NewGuid();
        var foodRef = Guid.NewGuid();
        var item = meal.AddItem(itemId, foodRef, 1, 1.5m);

        item.Id.ShouldBe(itemId);
        item.NutritionTemplateMealId.ShouldBe(meal.Id);
        item.FoodId.ShouldBe(foodRef);
        item.Order.ShouldBe(1);
        item.Quantity.ShouldBe(1.5m);
        meal.Items.Count.ShouldBe(1);
        meal.NutritionPlanTemplateId.ShouldBe(template.Id);
    }

    [Fact]
    public void Should_Clear_All_Meals()
    {
        var template = NutritionPlanTemplate.Create(Guid.NewGuid(), "Template");
        template.AddMeal(Guid.NewGuid(), "Breakfast", 1);
        template.AddMeal(Guid.NewGuid(), "Lunch", 2);

        template.ClearMeals();

        template.Meals.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Meal_Name_Is_NullOrWhitespace(string? mealName)
    {
        var template = NutritionPlanTemplate.Create(Guid.NewGuid(), "Template");

        Should.Throw<ArgumentException>(() => template.AddMeal(Guid.NewGuid(), mealName!, 1));
    }
}
