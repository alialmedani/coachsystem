using System;
using CoachApp.Entites.WorkoutPlans;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.WorkoutPlanTemplates;

/// <summary>
/// Pure domain tests for the <see cref="WorkoutPlanTemplate"/> aggregate: the <c>Create</c>
/// factory (no trainee / no active state) and the Day → Exercise build methods.
/// </summary>
public class WorkoutPlanTemplateTests
{
    [Fact]
    public void Should_Create_Template_With_No_Days()
    {
        var id = Guid.NewGuid();

        var template = WorkoutPlanTemplate.Create(id, "Beginner Full Body", "3-day");

        template.Id.ShouldBe(id);
        template.Name.ShouldBe("Beginner Full Body");
        template.Description.ShouldBe("3-day");
        template.Days.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_NullOrWhitespace(string? name)
    {
        Should.Throw<ArgumentException>(() => WorkoutPlanTemplate.Create(Guid.NewGuid(), name!));
    }

    [Fact]
    public void Should_Throw_When_Name_Exceeds_MaxLength()
    {
        var tooLong = new string('a', WorkoutPlanConsts.MaxNameLength + 1);

        Should.Throw<ArgumentException>(() => WorkoutPlanTemplate.Create(Guid.NewGuid(), tooLong));
    }

    [Fact]
    public void Should_Build_Full_Day_And_Exercise_Graph()
    {
        var template = WorkoutPlanTemplate.Create(Guid.NewGuid(), "Template");
        var day = template.AddDay(Guid.NewGuid(), "Day 1", 1);

        var exId = Guid.NewGuid();
        var exerciseRef = Guid.NewGuid();
        var ex = day.AddExercise(exId, exerciseRef, 1, 4, "8-12", 60m, 90, "tempo");

        ex.Id.ShouldBe(exId);
        ex.WorkoutTemplateDayId.ShouldBe(day.Id);
        ex.ExerciseId.ShouldBe(exerciseRef);
        ex.Sets.ShouldBe(4);
        ex.Reps.ShouldBe("8-12");
        ex.WeightKg.ShouldBe(60m);
        ex.RestSeconds.ShouldBe(90);
        ex.Notes.ShouldBe("tempo");
        day.Exercises.Count.ShouldBe(1);
        day.WorkoutPlanTemplateId.ShouldBe(template.Id);
    }

    [Fact]
    public void Should_Clear_All_Days()
    {
        var template = WorkoutPlanTemplate.Create(Guid.NewGuid(), "Template");
        template.AddDay(Guid.NewGuid(), "Day 1", 1);
        template.AddDay(Guid.NewGuid(), "Day 2", 2);

        template.ClearDays();

        template.Days.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Day_Name_Is_NullOrWhitespace(string? dayName)
    {
        var template = WorkoutPlanTemplate.Create(Guid.NewGuid(), "Template");

        Should.Throw<ArgumentException>(() => template.AddDay(Guid.NewGuid(), dayName!, 1));
    }
}
