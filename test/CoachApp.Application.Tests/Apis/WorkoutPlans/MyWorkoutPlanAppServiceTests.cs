using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.WorkoutPlans;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.WorkoutPlans;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyWorkoutPlanAppService"/> (read-only).
/// Plans are seeded through the coach service, then read impersonating the trainee via ChangeToTrainee.
/// Permission-level authorization (403) is not exercised (AlwaysAllowAuthorization); these assert the
/// real enforcement: trainee resolved from the current user + per-plan ownership. Cross-tenant isolation
/// isn't directly testable here (single host tenant) — same-tenant cross-trainee isolation is.
/// </summary>
public abstract class MyWorkoutPlanAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyWorkoutPlanAppService _myPlan;
    private readonly IWorkoutPlanAppService _coachPlan;

    protected MyWorkoutPlanAppServiceTests()
    {
        _myPlan = GetRequiredService<IMyWorkoutPlanAppService>();
        _coachPlan = GetRequiredService<IWorkoutPlanAppService>();
    }

    [Fact]
    public async Task GetActive_Should_Return_The_Active_Plan_With_Enriched_Exercises()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync("Deadlift");

        await _coachPlan.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Strength",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new()
                {
                    Name = "Day 1",
                    Order = 1,
                    Exercises = new List<CreateUpdateWorkoutExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 3 } }
                }
            }
        });

        WorkoutPlanDto? active;
        using (ChangeToTrainee(trainee))
        {
            active = await _myPlan.GetActiveAsync();
        }

        active.ShouldNotBeNull();
        active!.Name.ShouldBe("Strength");
        active.Days.ShouldNotBeEmpty();
        active.Days.Single().Exercises.Single().ExerciseName.ShouldBe("Deadlift"); // enriched
    }

    [Fact]
    public async Task GetActive_Should_Return_Null_When_No_Active_Plan()
    {
        var trainee = await CreateTraineeAsync();
        await _coachPlan.CreateAsync(new CreateUpdateWorkoutPlanDto { TraineeId = trainee.Id, Name = "Draft", IsActive = false });

        using (ChangeToTrainee(trainee))
        {
            (await _myPlan.GetActiveAsync()).ShouldBeNull();
        }
    }

    [Fact]
    public async Task GetList_Should_Return_Only_Own_Plans()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        await _coachPlan.CreateAsync(new CreateUpdateWorkoutPlanDto { TraineeId = trainee.Id, Name = "Mine" });
        await _coachPlan.CreateAsync(new CreateUpdateWorkoutPlanDto { TraineeId = other.Id, Name = "Theirs" });

        using (ChangeToTrainee(trainee))
        {
            var list = await _myPlan.GetListAsync();
            list.Count.ShouldBe(1);
            list.Single().TraineeId.ShouldBe(trainee.Id);
        }
    }

    [Fact]
    public async Task Get_Should_Throw_For_A_Plan_Owned_By_Another_Trainee()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        var plan = await _coachPlan.CreateAsync(new CreateUpdateWorkoutPlanDto { TraineeId = trainee.Id, Name = "Mine" });

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _myPlan.GetAsync(plan.Id));
        }
    }
}
