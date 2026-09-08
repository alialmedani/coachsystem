using System;
using Shouldly;
using Xunit;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>
/// Pure domain tests for the <see cref="NutritionLog"/> aggregate's <c>Create</c>
/// factory and its <c>AddEntry</c> behaviour method.
/// </summary>
public class NutritionLogTests
{
    [Fact]
    public void Should_Create_Log_With_No_Entries()
    {
        var id = Guid.NewGuid();
        var traineeId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var date = new DateTime(2026, 3, 2);

        var log = NutritionLog.Create(id, traineeId, date, planId, "On plan today");

        log.Id.ShouldBe(id);
        log.TraineeId.ShouldBe(traineeId);
        log.Date.ShouldBe(date);
        log.NutritionPlanId.ShouldBe(planId);
        log.Notes.ShouldBe("On plan today");
        log.Entries.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Add_Entry_With_All_Fields()
    {
        var log = NutritionLog.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now);

        var entryId = Guid.NewGuid();
        var foodId = Guid.NewGuid();
        var entry = log.AddEntry(entryId, foodId, 1, 1.5m, "with sauce");

        entry.Id.ShouldBe(entryId);
        entry.NutritionLogId.ShouldBe(log.Id);
        entry.FoodId.ShouldBe(foodId);
        entry.Order.ShouldBe(1);
        entry.Quantity.ShouldBe(1.5m);
        entry.Notes.ShouldBe("with sauce");
        log.Entries.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_Throw_When_Notes_Exceeds_MaxLength()
    {
        var tooLong = new string('a', NutritionLogConsts.MaxNotesLength + 1);

        Should.Throw<ArgumentException>(() =>
            NutritionLog.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.Now, notes: tooLong));
    }
}
