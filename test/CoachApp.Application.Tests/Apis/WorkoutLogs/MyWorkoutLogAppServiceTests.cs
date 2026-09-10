using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Apis.WorkoutPlans;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.WorkoutLogs;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyWorkoutLogAppService"/> "Log From Plan"
/// flow: creating a session snapshotted from a plan day (prescribed + actual values, ownership),
/// and editing a logged session. All log calls run impersonating the trainee via ChangeToTrainee.
/// </summary>
public abstract class MyWorkoutLogAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyWorkoutLogAppService _myLog;
    private readonly IWorkoutPlanAppService _planAppService;

    protected MyWorkoutLogAppServiceTests()
    {
        _myLog = GetRequiredService<IMyWorkoutLogAppService>();
        _planAppService = GetRequiredService<IWorkoutPlanAppService>();
    }

    private async Task<(Entites.Trainees.TraineeDto Trainee, WorkoutPlanDto Plan, Guid DayId)> SeedTraineeWithPlanDayAsync()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync("Bench Press");

        var plan = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new()
                {
                    Name = "Day 1 - Push",
                    Order = 1,
                    ScheduledDay = DayOfWeek.Monday,
                    Exercises = new List<CreateUpdateWorkoutExerciseDto>
                    {
                        new() { ExerciseId = ex.Id, Order = 1, Sets = 3, Reps = "8-12", WeightKg = 60m, Notes = "tempo" }
                    }
                }
            }
        });

        return (trainee, plan, plan.Days.Single().Id);
    }

    [Fact]
    public async Task CreateFromDay_Should_Snapshot_Plan_Day_Into_Log()
    {
        var (trainee, plan, dayId) = await SeedTraineeWithPlanDayAsync();

        WorkoutLogDto log;
        using (ChangeToTrainee(trainee))
        {
            log = await _myLog.CreateFromDayAsync(new CreateWorkoutLogFromDayDto
            {
                WorkoutDayId = dayId,
                Date = new DateTime(2026, 3, 2),
                Notes = "session"
            });
        }

        log.WorkoutPlanId.ShouldBe(plan.Id);
        log.WorkoutDayId.ShouldBe(dayId);
        log.Entries.Count.ShouldBe(1);

        var entry = log.Entries.Single();
        // Actuals seeded from the plan.
        entry.Sets.ShouldBe(3);
        entry.Reps.ShouldBe("8-12");
        entry.WeightKg.ShouldBe(60m);
        entry.ExerciseName.ShouldBe("Bench Press"); // enriched
        // Prescribed snapshot.
        entry.PrescribedSets.ShouldBe(3);
        entry.PrescribedReps.ShouldBe("8-12");
        entry.PrescribedWeightKg.ShouldBe(60m);
    }

    [Fact]
    public async Task CreateFromDay_Should_Throw_When_Day_Belongs_To_Another_Trainee()
    {
        var (_, _, dayId) = await SeedTraineeWithPlanDayAsync();
        var otherTrainee = await CreateTraineeAsync();

        using (ChangeToTrainee(otherTrainee))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.CreateFromDayAsync(new CreateWorkoutLogFromDayDto
                {
                    WorkoutDayId = dayId,
                    Date = new DateTime(2026, 3, 2)
                }));
        }
    }

    [Fact]
    public async Task CreateFromDay_Should_Throw_When_Day_Is_Unknown()
    {
        var trainee = await CreateTraineeAsync();

        using (ChangeToTrainee(trainee))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.CreateFromDayAsync(new CreateWorkoutLogFromDayDto
                {
                    WorkoutDayId = Guid.NewGuid(),
                    Date = new DateTime(2026, 3, 2)
                }));
        }
    }

    [Fact]
    public async Task Update_Should_Replace_Actuals_And_Preserve_Prescribed()
    {
        var (trainee, _, dayId) = await SeedTraineeWithPlanDayAsync();

        WorkoutLogDto updated;
        using (ChangeToTrainee(trainee))
        {
            var log = await _myLog.CreateFromDayAsync(new CreateWorkoutLogFromDayDto
            {
                WorkoutDayId = dayId,
                Date = new DateTime(2026, 3, 2)
            });

            var entry = log.Entries.Single();

            updated = await _myLog.UpdateAsync(log.Id, new UpdateWorkoutLogDto
            {
                Date = new DateTime(2026, 3, 2),
                Notes = "done",
                Entries = new List<UpdateWorkoutLogEntryDto>
                {
                    new()
                    {
                        ExerciseId = entry.ExerciseId,
                        Order = 1,
                        Sets = 3,
                        Reps = "8",
                        WeightKg = 65m,           // actual: heavier than prescribed
                        Notes = "hit it"
                    }
                }
            });
        }

        updated.Notes.ShouldBe("done");
        var updatedEntry = updated.Entries.Single();
        updatedEntry.WeightKg.ShouldBe(65m);            // actual updated
        updatedEntry.Reps.ShouldBe("8");
        updatedEntry.PrescribedWeightKg.ShouldBe(60m);  // prescribed preserved
        updatedEntry.PrescribedReps.ShouldBe("8-12");
    }

    [Fact]
    public async Task Update_Should_Throw_For_A_Log_Owned_By_Another_Trainee()
    {
        var (trainee, _, dayId) = await SeedTraineeWithPlanDayAsync();
        var otherTrainee = await CreateTraineeAsync();

        Guid logId;
        using (ChangeToTrainee(trainee))
        {
            var log = await _myLog.CreateFromDayAsync(new CreateWorkoutLogFromDayDto
            {
                WorkoutDayId = dayId,
                Date = new DateTime(2026, 3, 2)
            });
            logId = log.Id;
        }

        using (ChangeToTrainee(otherTrainee))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.UpdateAsync(logId, new UpdateWorkoutLogDto { Date = new DateTime(2026, 3, 2) }));
        }
    }

    [Fact]
    public async Task Update_Should_Not_Let_Trainee_Change_Prescribed_Snapshot()
    {
        var (trainee, _, dayId) = await SeedTraineeWithPlanDayAsync();

        WorkoutLogDto updated;
        using (ChangeToTrainee(trainee))
        {
            var log = await _myLog.CreateFromDayAsync(new CreateWorkoutLogFromDayDto
            {
                WorkoutDayId = dayId,
                Date = new DateTime(2026, 3, 2)
            });
            var entry = log.Entries.Single();

            // The update DTO no longer exposes prescribed fields, so the trainee can only change
            // actuals — there is no channel to overwrite the server-owned prescribed snapshot.
            updated = await _myLog.UpdateAsync(log.Id, new UpdateWorkoutLogDto
            {
                Date = new DateTime(2026, 3, 2),
                Entries = new List<UpdateWorkoutLogEntryDto>
                {
                    new()
                    {
                        ExerciseId = entry.ExerciseId,
                        Order = 1,
                        Sets = 10,
                        Reps = "1",
                        WeightKg = 200m
                    }
                }
            });
        }

        var updatedEntry = updated.Entries.Single();
        // Actuals changed…
        updatedEntry.Sets.ShouldBe(10);
        updatedEntry.WeightKg.ShouldBe(200m);
        // …but the prescribed snapshot is unchanged from the plan (3 / 8-12 / 60).
        updatedEntry.PrescribedSets.ShouldBe(3);
        updatedEntry.PrescribedReps.ShouldBe("8-12");
        updatedEntry.PrescribedWeightKg.ShouldBe(60m);
    }

    [Fact]
    public async Task Create_With_Own_Plan_And_Day_Reference_Should_Succeed()
    {
        var (trainee, plan, dayId) = await SeedTraineeWithPlanDayAsync();
        var exerciseId = plan.Days.Single().Exercises.Single().ExerciseId;

        WorkoutLogDto log;
        using (ChangeToTrainee(trainee))
        {
            log = await _myLog.CreateAsync(new CreateWorkoutLogDto
            {
                WorkoutPlanId = plan.Id,
                WorkoutDayId = dayId,
                Date = new DateTime(2026, 3, 2),
                Entries = new List<CreateWorkoutLogEntryDto> { new() { ExerciseId = exerciseId, Order = 1, Sets = 3 } }
            });
        }

        log.WorkoutPlanId.ShouldBe(plan.Id);
        log.WorkoutDayId.ShouldBe(dayId);
        log.Entries.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Create_With_Foreign_Plan_Reference_Should_Throw()
    {
        var (trainee, _, _) = await SeedTraineeWithPlanDayAsync();
        var (_, foreignPlan, _) = await SeedTraineeWithPlanDayAsync();

        using (ChangeToTrainee(trainee))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.CreateAsync(new CreateWorkoutLogDto
                {
                    WorkoutPlanId = foreignPlan.Id,
                    Date = new DateTime(2026, 3, 2),
                    Entries = new List<CreateWorkoutLogEntryDto>()
                }));
        }
    }

    [Fact]
    public async Task Create_With_Foreign_Day_Reference_Should_Throw()
    {
        var (trainee, _, _) = await SeedTraineeWithPlanDayAsync();
        var (_, _, foreignDayId) = await SeedTraineeWithPlanDayAsync();

        using (ChangeToTrainee(trainee))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.CreateAsync(new CreateWorkoutLogDto
                {
                    WorkoutDayId = foreignDayId,
                    Date = new DateTime(2026, 3, 2),
                    Entries = new List<CreateWorkoutLogEntryDto>()
                }));
        }
    }

    [Fact]
    public async Task Create_With_Day_From_Another_Plan_Should_Throw()
    {
        var (trainee, plan1, _) = await SeedTraineeWithPlanDayAsync();

        // A second plan owned by the SAME trainee; its day must not be attachable under plan1's id.
        var plan2 = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Second",
            Days = new List<CreateUpdateWorkoutDayDto> { new() { Name = "Other", Order = 1 } }
        });
        var dayFromPlan2 = plan2.Days.Single().Id;

        using (ChangeToTrainee(trainee))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.CreateAsync(new CreateWorkoutLogDto
                {
                    WorkoutPlanId = plan1.Id,
                    WorkoutDayId = dayFromPlan2,
                    Date = new DateTime(2026, 3, 2),
                    Entries = new List<CreateWorkoutLogEntryDto>()
                }));
        }
    }

    [Fact]
    public async Task Create_Manual_Log_Without_Plan_References_Should_Succeed()
    {
        var (trainee, _, _) = await SeedTraineeWithPlanDayAsync();
        var ex = await CreateExerciseAsync("Curl");

        WorkoutLogDto log;
        using (ChangeToTrainee(trainee))
        {
            log = await _myLog.CreateAsync(new CreateWorkoutLogDto
            {
                Date = new DateTime(2026, 3, 2),
                Entries = new List<CreateWorkoutLogEntryDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 3 } }
            });
        }

        log.WorkoutPlanId.ShouldBeNull();
        log.WorkoutDayId.ShouldBeNull();
        log.Entries.Count.ShouldBe(1);
    }
}
