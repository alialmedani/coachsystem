using System;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.WorkoutLogs;

/// <summary>
/// Pure domain tests for the <see cref="WorkoutLog"/> aggregate's <c>Create</c>
/// factory and its <c>AddEntry</c> behaviour method.
/// </summary>
public class WorkoutLogTests
{
    [Fact]
    public void Should_Create_Log_With_No_Entries()
    {
        var id = Guid.NewGuid();
        var traineeId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var dayId = Guid.NewGuid();
        var date = new DateTime(2026, 3, 1);

        var log = WorkoutLog.Create(id, traineeId, date, planId, dayId, "Good session");

        log.Id.ShouldBe(id);
        log.TraineeId.ShouldBe(traineeId);
        log.Date.ShouldBe(date);
        log.WorkoutPlanId.ShouldBe(planId);
        log.WorkoutDayId.ShouldBe(dayId);
        log.Notes.ShouldBe("Good session");
        log.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Add_Entry_With_All_Fields()
    {
        var log = WorkoutLog.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

        var entryId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();
        var entry = log.AddEntry(entryId, exerciseId, 1, 5, "5", 100m, "PR");

        entry.Id.ShouldBe(entryId);
        entry.WorkoutLogId.ShouldBe(log.Id);
        entry.ExerciseId.ShouldBe(exerciseId);
        entry.Order.ShouldBe(1);
        entry.Sets.ShouldBe(5);
        entry.Reps.ShouldBe("5");
        entry.WeightKg.ShouldBe(100m);
        entry.Notes.ShouldBe("PR");
        log.Entries.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Throw_When_Notes_Exceeds_MaxLength()
    {
        var tooLong = new string('a', WorkoutLogConsts.MaxNotesLength + 1);

        Should.Throw<ArgumentException>(() =>
            WorkoutLog.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, notes: tooLong));
    }
}
