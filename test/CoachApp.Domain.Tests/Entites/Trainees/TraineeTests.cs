using System;
using CoachApp.Enums;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.Trainees;

/// <summary>
/// Pure domain tests for the <see cref="Trainee"/> aggregate's <c>Create</c> factory.
/// No DB/DI required — <c>Create</c> is a pure validating constructor.
/// </summary>
public class TraineeTests
{
    [Fact]
    public void Should_Create_Trainee_When_Required_Fields_Are_Valid()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var trainee = Trainee.Create(
            id,
            userId,
            "john.doe",
            "John",
            "Doe",
            Gender.Male,
            new DateTime(1990, 1, 1),
            "john@doe.com",
            "0123456789",
            TrainingGoal.BuildMuscle,
            180m,
            80m,
            75m);

        trainee.Id.ShouldBe(id);
        trainee.UserId.ShouldBe(userId);
        trainee.UserName.ShouldBe("john.doe");
        trainee.FirstName.ShouldBe("John");
        trainee.LastName.ShouldBe("Doe");
        trainee.Gender.ShouldBe(Gender.Male);
        trainee.Goal.ShouldBe(TrainingGoal.BuildMuscle);
        trainee.HeightCm.ShouldBe(180m);
        trainee.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_UserName_Is_NullOrWhitespace(string? userName)
    {
        Should.Throw<ArgumentException>(() =>
            Trainee.Create(Guid.NewGuid(), Guid.NewGuid(), userName!, "John", "Doe"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_FirstName_Is_NullOrWhitespace(string? firstName)
    {
        Should.Throw<ArgumentException>(() =>
            Trainee.Create(Guid.NewGuid(), Guid.NewGuid(), "john.doe", firstName!, "Doe"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_LastName_Is_NullOrWhitespace(string? lastName)
    {
        Should.Throw<ArgumentException>(() =>
            Trainee.Create(Guid.NewGuid(), Guid.NewGuid(), "john.doe", "John", lastName!));
    }

    [Fact]
    public void Should_Throw_When_FirstName_Exceeds_MaxLength()
    {
        var tooLong = new string('a', TraineeConsts.MaxFirstNameLength + 1);

        Should.Throw<ArgumentException>(() =>
            Trainee.Create(Guid.NewGuid(), Guid.NewGuid(), "john.doe", tooLong, "Doe"));
    }

    [Fact]
    public void Should_Deactivate_And_Activate()
    {
        var trainee = Trainee.Create(Guid.NewGuid(), Guid.NewGuid(), "john.doe", "John", "Doe");

        trainee.Deactivate();
        trainee.IsActive.ShouldBeFalse();

        trainee.Activate();
        trainee.IsActive.ShouldBeTrue();
    }
}
