using System;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.TraineeNotes;

/// <summary>Pure domain tests for the <see cref="TraineeNote"/> aggregate's <c>Create</c> factory.</summary>
public class TraineeNoteTests
{
    [Fact]
    public void Should_Create_Note_When_Text_Is_Valid()
    {
        var id = Guid.NewGuid();
        var traineeId = Guid.NewGuid();
        var date = new DateTime(2026, 1, 15);

        var note = TraineeNote.Create(id, traineeId, date, "Great progress this week!");

        note.Id.ShouldBe(id);
        note.TraineeId.ShouldBe(traineeId);
        note.Date.ShouldBe(date);
        note.Text.ShouldBe("Great progress this week!");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_Text_Is_NullOrWhitespace(string? text)
    {
        Should.Throw<ArgumentException>(() =>
            TraineeNote.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, text!));
    }

    [Fact]
    public void Should_Throw_When_Text_Exceeds_MaxLength()
    {
        var tooLong = new string('a', TraineeNoteConsts.MaxTextLength + 1);

        Should.Throw<ArgumentException>(() =>
            TraineeNote.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, tooLong));
    }
}
