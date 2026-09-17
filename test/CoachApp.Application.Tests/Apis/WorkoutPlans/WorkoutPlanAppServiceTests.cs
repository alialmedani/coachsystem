using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.WorkoutPlans;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.WorkoutPlans;

/// <summary>
/// Integration tests for the coach-facing <see cref="IWorkoutPlanAppService"/>: the
/// nested Plan → Day → Exercise graph, cross-aggregate existence checks, and the
/// "at most one active plan per trainee" rule.
/// </summary>
public abstract class WorkoutPlanAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IWorkoutPlanAppService _planAppService;

    protected WorkoutPlanAppServiceTests()
    {
        _planAppService = GetRequiredService<IWorkoutPlanAppService>();
    }

    [Fact]
    public async Task Should_Create_Plan_With_Nested_Days_And_Exercises()
    {
        var trainee = await CreateTraineeAsync();
        var ex1 = await CreateExerciseAsync("Bench Press");
        var ex2 = await CreateExerciseAsync("Incline Press");

        var result = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Upper/Lower",
            Description = "4-day",
            IsActive = false,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new()
                {
                    Name = "Day 1 - Upper",
                    Order = 1,
                    Exercises = new List<CreateUpdateWorkoutExerciseDto>
                    {
                        new() { ExerciseId = ex1.Id, Order = 1, Sets = 4, Reps = "8-12", WeightKg = 60m },
                        new() { ExerciseId = ex2.Id, Order = 2, Sets = 3, Reps = "10" }
                    }
                }
            }
        });

        result.Id.ShouldNotBe(Guid.Empty);
        result.TraineeId.ShouldBe(trainee.Id);
        result.Days.Count.ShouldBe(1);

        var day = result.Days.Single();
        day.Name.ShouldBe("Day 1 - Upper");
        day.Exercises.Count.ShouldBe(2);

        var first = day.Exercises.Single(e => e.ExerciseId == ex1.Id);
        first.Sets.ShouldBe(4);
        first.Reps.ShouldBe("8-12");
        first.WeightKg.ShouldBe(60m);
        first.ExerciseName.ShouldBe("Bench Press"); // enriched from the library
    }

    [Fact]
    public async Task Should_Throw_When_Trainee_Does_Not_Exist()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
            {
                TraineeId = Guid.NewGuid(),
                Name = "Orphan Plan"
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.TraineeNotFound);
    }

    [Fact]
    public async Task Should_Throw_When_Referencing_Unknown_Exercise()
    {
        var trainee = await CreateTraineeAsync();

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
            {
                TraineeId = trainee.Id,
                Name = "Bad Plan",
                Days = new List<CreateUpdateWorkoutDayDto>
                {
                    new()
                    {
                        Name = "Day 1",
                        Order = 1,
                        Exercises = new List<CreateUpdateWorkoutExerciseDto>
                        {
                            new() { ExerciseId = Guid.NewGuid(), Order = 1, Sets = 3 }
                        }
                    }
                }
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.ExercisesNotFound);
    }

    [Fact]
    public async Task Should_Deactivate_Other_Plans_When_A_Plan_Is_Set_Active()
    {
        var trainee = await CreateTraineeAsync();

        var plan1 = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan 1",
            IsActive = true
        });
        plan1.IsActive.ShouldBeTrue();

        var plan2 = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan 2",
            IsActive = false
        });

        var activated = await _planAppService.SetActiveAsync(plan2.Id);
        activated.IsActive.ShouldBeTrue();

        var plan1After = await _planAppService.GetAsync(plan1.Id);
        plan1After.IsActive.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Keep_Only_One_Active_Plan_When_Created_Active()
    {
        var trainee = await CreateTraineeAsync();

        var plan1 = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan A",
            IsActive = true
        });

        var plan2 = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan B",
            IsActive = true
        });

        (await _planAppService.GetAsync(plan1.Id)).IsActive.ShouldBeFalse();
        (await _planAppService.GetAsync(plan2.Id)).IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Replace_Day_Structure_On_Update()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync();

        var created = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan",
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Day 1", Order = 1 }
            }
        });
        created.Days.Count.ShouldBe(1);

        var updated = await _planAppService.UpdateAsync(created.Id, new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan",
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Day 1", Order = 1, Exercises = new List<CreateUpdateWorkoutExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 3 } } },
                new() { Name = "Day 2", Order = 2 }
            }
        });

        updated.Days.Count.ShouldBe(2);
        updated.Days.OrderBy(d => d.Order).First().Exercises.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Persist_ScheduledDay_Through_Create_And_Update()
    {
        var trainee = await CreateTraineeAsync();

        var created = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Weekly Split",
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Upper", Order = 1, ScheduledDay = DayOfWeek.Monday },
                new() { Name = "Lower", Order = 2, ScheduledDay = null }
            }
        });

        created.Days.Single(d => d.Name == "Upper").ScheduledDay.ShouldBe(DayOfWeek.Monday);
        created.Days.Single(d => d.Name == "Lower").ScheduledDay.ShouldBeNull();

        var updated = await _planAppService.UpdateAsync(created.Id, new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Weekly Split",
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Upper", Order = 1, ScheduledDay = DayOfWeek.Wednesday }
            }
        });

        updated.Days.Single().ScheduledDay.ShouldBe(DayOfWeek.Wednesday);
    }

    [Fact]
    public async Task Should_List_Plans_Filtered_By_Trainee()
    {
        var trainee1 = await CreateTraineeAsync();
        var trainee2 = await CreateTraineeAsync();

        await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto { TraineeId = trainee1.Id, Name = "T1 Plan" });
        await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto { TraineeId = trainee2.Id, Name = "T2 Plan" });

        var result = await _planAppService.GetListAsync(new GetWorkoutPlanListInput { TraineeId = trainee1.Id });

        result.TotalCount.ShouldBe(1);
        result.Items.Single().TraineeId.ShouldBe(trainee1.Id);
    }

    [Fact]
    public async Task Should_Delete_Plan()
    {
        var trainee = await CreateTraineeAsync();
        var created = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto { TraineeId = trainee.Id, Name = "Temp" });

        await _planAppService.DeleteAsync(created.Id);

        var result = await _planAppService.GetListAsync(new GetWorkoutPlanListInput { TraineeId = trainee.Id });
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Reject_Duplicate_Scheduled_Weekday_On_Create()
    {
        // F2/PD9: at most one scheduled workout per weekday.
        var trainee = await CreateTraineeAsync();

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
            {
                TraineeId = trainee.Id,
                Name = "Double Monday",
                Days = new List<CreateUpdateWorkoutDayDto>
                {
                    new() { Name = "AM", Order = 1, ScheduledDay = DayOfWeek.Monday },
                    new() { Name = "PM", Order = 2, ScheduledDay = DayOfWeek.Monday }
                }
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.DuplicateScheduledDay);
    }

    [Fact]
    public async Task Should_Reject_Duplicate_Scheduled_Weekday_On_Update()
    {
        var trainee = await CreateTraineeAsync();
        var created = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Split",
            Days = new List<CreateUpdateWorkoutDayDto> { new() { Name = "Day 1", Order = 1, ScheduledDay = DayOfWeek.Monday } }
        });

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _planAppService.UpdateAsync(created.Id, new CreateUpdateWorkoutPlanDto
            {
                TraineeId = trainee.Id,
                Name = "Split",
                Days = new List<CreateUpdateWorkoutDayDto>
                {
                    new() { Name = "Day 1", Order = 1, ScheduledDay = DayOfWeek.Tuesday },
                    new() { Name = "Day 2", Order = 2, ScheduledDay = DayOfWeek.Tuesday }
                }
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.DuplicateScheduledDay);
    }

    [Fact]
    public async Task Should_Allow_Multiple_Unscheduled_Days()
    {
        // Only non-null weekdays must be unique; unscheduled (null) days are unlimited.
        var trainee = await CreateTraineeAsync();

        var created = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Flexible",
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "A", Order = 1, ScheduledDay = null },
                new() { Name = "B", Order = 2, ScheduledDay = null },
                new() { Name = "C", Order = 3, ScheduledDay = DayOfWeek.Friday }
            }
        });

        created.Days.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Update_Should_Not_Reassign_Plan_To_A_Different_Trainee()
    {
        // F18/PD1: a plan is permanently bound to its trainee; a changed TraineeId on update is ignored.
        var trainee1 = await CreateTraineeAsync();
        var trainee2 = await CreateTraineeAsync();

        var created = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee1.Id,
            Name = "Owned by T1"
        });

        var updated = await _planAppService.UpdateAsync(created.Id, new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee2.Id, // attempt to hijack
            Name = "Renamed"
        });

        updated.TraineeId.ShouldBe(trainee1.Id);
        (await _planAppService.GetAsync(created.Id)).TraineeId.ShouldBe(trainee1.Id);

        // And it never appears on trainee2's list.
        (await _planAppService.GetListAsync(new GetWorkoutPlanListInput { TraineeId = trainee2.Id })).TotalCount.ShouldBe(0);
    }
}
