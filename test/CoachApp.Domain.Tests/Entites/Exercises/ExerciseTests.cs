using System;
using CoachApp.Enums;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.Exercises;

/// <summary>Pure domain tests for the <see cref="Exercise"/> aggregate's <c>Create</c> factory.</summary>
public class ExerciseTests
{
    [Fact]
    public void Should_Create_Exercise_When_Name_Is_Valid()
    {
        var id = Guid.NewGuid();

        var exercise = Exercise.Create(
            id,
            "Bench Press",
            MuscleGroup.Chest,
            Equipment.Barbell,
            "A chest exercise",
            "Lie on the bench and press",
            "https://video",
            "https://image");

        exercise.Id.ShouldBe(id);
        exercise.Name.ShouldBe("Bench Press");
        exercise.TargetMuscle.ShouldBe(MuscleGroup.Chest);
        exercise.Equipment.ShouldBe(Equipment.Barbell);
        exercise.Description.ShouldBe("A chest exercise");
        exercise.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_Default_Muscle_And_Equipment_When_Not_Specified()
    {
        var exercise = Exercise.Create(Guid.NewGuid(), "Plank");

        exercise.TargetMuscle.ShouldBe(MuscleGroup.Other);
        exercise.Equipment.ShouldBe(Equipment.None);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Name_Is_NullOrWhitespace(string? name)
    {
        Should.Throw<ArgumentException>(() => Exercise.Create(Guid.NewGuid(), name!));
    }

    [Fact]
    public void Should_Throw_When_Name_Exceeds_MaxLength()
    {
        var tooLong = new string('a', ExerciseConsts.MaxNameLength + 1);

        Should.Throw<ArgumentException>(() => Exercise.Create(Guid.NewGuid(), tooLong));
    }

    [Fact]
    public void Should_Throw_When_Description_Exceeds_MaxLength()
    {
        var tooLong = new string('a', ExerciseConsts.MaxDescriptionLength + 1);

        Should.Throw<ArgumentException>(() =>
            Exercise.Create(Guid.NewGuid(), "Squat", description: tooLong));
    }
}
