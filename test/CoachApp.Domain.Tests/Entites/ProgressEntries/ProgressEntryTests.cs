using System;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.ProgressEntries;

/// <summary>Pure domain tests for the <see cref="ProgressEntry"/> aggregate's <c>Create</c> factory.</summary>
public class ProgressEntryTests
{
    [Fact]
    public void Should_Create_Entry_With_All_Metrics()
    {
        var id = Guid.NewGuid();
        var traineeId = Guid.NewGuid();
        var date = new DateTime(2026, 2, 1);

        var entry = ProgressEntry.Create(
            id, traineeId, date,
            weightKg: 82.5m,
            bodyFatPercent: 18m,
            chestCm: 100m,
            waistCm: 85m,
            hipsCm: 95m,
            armCm: 38m,
            thighCm: 60m,
            notes: "Feeling strong");

        entry.Id.ShouldBe(id);
        entry.TraineeId.ShouldBe(traineeId);
        entry.Date.ShouldBe(date);
        entry.WeightKg.ShouldBe(82.5m);
        entry.BodyFatPercent.ShouldBe(18m);
        entry.WaistCm.ShouldBe(85m);
        entry.Notes.ShouldBe("Feeling strong");
    }

    [Fact]
    public void Should_Create_Entry_With_Only_Required_Fields()
    {
        var entry = ProgressEntry.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

        entry.WeightKg.ShouldBeNull();
        entry.BodyFatPercent.ShouldBeNull();
        entry.Notes.ShouldBeNull();
    }

    [Fact]
    public void Should_Throw_When_Notes_Exceeds_MaxLength()
    {
        var tooLong = new string('a', ProgressEntryConsts.MaxNotesLength + 1);

        Should.Throw<ArgumentException>(() =>
            ProgressEntry.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, notes: tooLong));
    }
}
