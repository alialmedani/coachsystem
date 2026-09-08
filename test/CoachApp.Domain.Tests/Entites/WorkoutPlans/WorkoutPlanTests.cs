using System;
using System.Linq;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.WorkoutPlans;

/// <summary>
/// Pure domain tests for the <see cref="WorkoutPlan"/> aggregate: the <c>Create</c>
/// factory and the behaviour methods that build the Plan → Day → Exercise graph.
/// </summary>
public class WorkoutPlanTests
{
    [Fact]
    public void Should_Create_Plan_Inactive_With_No_Days()
    {
        var id = Guid.NewGuid();
        var traineeId = Guid.NewGuid();

        var plan = WorkoutPlan.Create(id, traineeId, "Push Pull Legs", "6-day split");

        plan.Id.ShouldBe(id);
        plan.TraineeId.ShouldBe(traineeId);
        plan.Name.ShouldBe("Push Pull Legs");
        plan.Description.ShouldBe("6-day split");
        plan.IsActive.ShouldBeFalse();
        plan.Days.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_NullOrWhitespace(string? name)
    {
        Should.Throw<ArgumentException>(() =>
            WorkoutPlan.Create(Guid.NewGuid(), Guid.NewGuid(), name!));
    }

    [Fact]
    public void Should_Throw_When_Name_Exceeds_MaxLength()
    {
        var tooLong = new string('a', WorkoutPlanConsts.MaxNameLength + 1);

        Should.Throw<ArgumentException>(() =>
            WorkoutPlan.Create(Guid.NewGuid(), Guid.NewGuid(), tooLong));
    }

    [Fact]
    public void Should_Add_Day_To_Plan()
    {
        var plan = WorkoutPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");
        var dayId = Guid.NewGuid();

        var day = plan.AddDay(dayId, "Day 1 - Push", 1);

        day.Id.ShouldBe(dayId);
        day.WorkoutPlanId.ShouldBe(plan.Id);
        day.Name.ShouldBe("Day 1 - Push");
        day.Order.ShouldBe(1);
        plan.Days.Count.ShouldBe(1);
        plan.Days.ShouldContain(day);
    }

    [Fact]
    public void Should_Build_Full_Day_And_Exercise_Graph()
    {
        var plan = WorkoutPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");
        var day = plan.AddDay(Guid.NewGuid(), "Day 1", 1);

        var exId = Guid.NewGuid();
        var exerciseRef = Guid.NewGuid();
        var workoutExercise = day.AddExercise(exId, exerciseRef, 1, 4, "8-12", 60m, 90, "slow tempo");

        workoutExercise.Id.ShouldBe(exId);
        workoutExercise.WorkoutDayId.ShouldBe(day.Id);
        workoutExercise.ExerciseId.ShouldBe(exerciseRef);
        workoutExercise.Order.ShouldBe(1);
        workoutExercise.Sets.ShouldBe(4);
        workoutExercise.Reps.ShouldBe("8-12");
        workoutExercise.WeightKg.ShouldBe(60m);
        workoutExercise.RestSeconds.ShouldBe(90);
        workoutExercise.Notes.ShouldBe("slow tempo");
        day.Exercises.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Clear_All_Days()
    {
        var plan = WorkoutPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");
        plan.AddDay(Guid.NewGuid(), "Day 1", 1);
        plan.AddDay(Guid.NewGuid(), "Day 2", 2);

        plan.ClearDays();

        plan.Days.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Toggle_Active_State()
    {
        var plan = WorkoutPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");

        plan.Activate();
        plan.IsActive.ShouldBeTrue();

        plan.Deactivate();
        plan.IsActive.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Day_Name_Is_NullOrWhitespace(string? dayName)
    {
        var plan = WorkoutPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "Plan");

        Should.Throw<ArgumentException>(() => plan.AddDay(Guid.NewGuid(), dayName!, 1));
    }
}
