using System;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.Foods;

/// <summary>Pure domain tests for the <see cref="Food"/> aggregate's <c>Create</c> factory.</summary>
public class FoodTests
{
    [Fact]
    public void Should_Create_Food_When_Required_Fields_Are_Valid()
    {
        var id = Guid.NewGuid();

        var food = Food.Create(
            id,
            "Chicken Breast",
            100m,
            "g",
            165m,
            31m,
            0m,
            3.6m,
            "Lean protein");

        food.Id.ShouldBe(id);
        food.Name.ShouldBe("Chicken Breast");
        food.ServingSize.ShouldBe(100m);
        food.ServingUnit.ShouldBe("g");
        food.Calories.ShouldBe(165m);
        food.ProteinG.ShouldBe(31m);
        food.CarbsG.ShouldBe(0m);
        food.FatG.ShouldBe(3.6m);
        food.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_NullOrWhitespace(string? name)
    {
        Should.Throw<ArgumentException>(() =>
            Food.Create(Guid.NewGuid(), name!, 100m, "g", 100m, 10m, 10m, 10m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_ServingUnit_Is_NullOrWhitespace(string? servingUnit)
    {
        Should.Throw<ArgumentException>(() =>
            Food.Create(Guid.NewGuid(), "Rice", 100m, servingUnit!, 100m, 10m, 10m, 10m));
    }

    [Fact]
    public void Should_Throw_When_Name_Exceeds_MaxLength()
    {
        var tooLong = new string('a', FoodConsts.MaxNameLength + 1);

        Should.Throw<ArgumentException>(() =>
            Food.Create(Guid.NewGuid(), tooLong, 100m, "g", 100m, 10m, 10m, 10m));
    }
}
