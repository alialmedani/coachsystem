using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Today;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Entites.WorkoutPlanTemplates;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.WorkoutPlanTemplates;

/// <summary>
/// Integration tests for the coach-facing <see cref="IWorkoutPlanTemplateAppService"/>:
/// template CRUD, save-an-existing-plan-as-template, and clone-template-onto-a-trainee
/// (which must produce a new, inactive plan).
/// </summary>
public abstract class WorkoutPlanTemplateAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IWorkoutPlanTemplateAppService _templateAppService;
    private readonly IWorkoutPlanAppService _planAppService;

    protected WorkoutPlanTemplateAppServiceTests()
    {
        _templateAppService = GetRequiredService<IWorkoutPlanTemplateAppService>();
        _planAppService = GetRequiredService<IWorkoutPlanAppService>();
    }

    [Fact]
    public async Task Should_Create_Template_With_Days_And_Exercises()
    {
        var ex1 = await CreateExerciseAsync("Squat");
        var ex2 = await CreateExerciseAsync("Deadlift");

        var result = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto
        {
            Name = "Full Body A",
            Description = "beginner",
            Days = new List<CreateUpdateWorkoutTemplateDayDto>
            {
                new()
                {
                    Name = "Day 1",
                    Order = 1,
                    Exercises = new List<CreateUpdateWorkoutTemplateExerciseDto>
                    {
                        new() { ExerciseId = ex1.Id, Order = 1, Sets = 5, Reps = "5" },
                        new() { ExerciseId = ex2.Id, Order = 2, Sets = 1, Reps = "5" }
                    }
                }
            }
        });

        result.Id.ShouldNotBe(Guid.Empty);
        result.Days.Count.ShouldBe(1);
        var day = result.Days.Single();
        day.Exercises.Count.ShouldBe(2);
        day.Exercises.Single(e => e.ExerciseId == ex1.Id).ExerciseName.ShouldBe("Squat"); // enriched
    }

    [Fact]
    public async Task Should_Throw_When_Referencing_Unknown_Exercise()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto
            {
                Name = "Bad",
                Days = new List<CreateUpdateWorkoutTemplateDayDto>
                {
                    new()
                    {
                        Name = "Day 1",
                        Order = 1,
                        Exercises = new List<CreateUpdateWorkoutTemplateExerciseDto>
                        {
                            new() { ExerciseId = Guid.NewGuid(), Order = 1, Sets = 3 }
                        }
                    }
                }
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.ExercisesNotFound);
    }

    [Fact]
    public async Task Should_Replace_Days_On_Update()
    {
        var ex = await CreateExerciseAsync();
        var created = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto
        {
            Name = "T",
            Days = new List<CreateUpdateWorkoutTemplateDayDto> { new() { Name = "Day 1", Order = 1 } }
        });
        created.Days.Count.ShouldBe(1);

        var updated = await _templateAppService.UpdateAsync(created.Id, new CreateUpdateWorkoutPlanTemplateDto
        {
            Name = "T",
            Days = new List<CreateUpdateWorkoutTemplateDayDto>
            {
                new() { Name = "Day 1", Order = 1, Exercises = new List<CreateUpdateWorkoutTemplateExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 3 } } },
                new() { Name = "Day 2", Order = 2 }
            }
        });

        updated.Days.Count.ShouldBe(2);
        updated.Days.OrderBy(d => d.Order).First().Exercises.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_Save_Existing_Plan_As_Template()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync("Bench");

        var plan = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Sourced Plan",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new() { Name = "Day 1", Order = 1, Exercises = new List<CreateUpdateWorkoutExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 4, Reps = "8" } } }
            }
        });

        var template = await _templateAppService.SaveAsTemplateAsync(new SaveWorkoutPlanAsTemplateDto
        {
            WorkoutPlanId = plan.Id,
            Name = "From Plan",
            Description = "snapshot"
        });

        template.Name.ShouldBe("From Plan");
        template.Days.Count.ShouldBe(1);
        template.Days.Single().Exercises.Single().ExerciseId.ShouldBe(ex.Id);
        template.Days.Single().Exercises.Single().Sets.ShouldBe(4);
    }

    [Fact]
    public async Task Should_Clone_Template_To_Trainee_As_Inactive_Plan()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync("OHP");

        var template = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto
        {
            Name = "Cloneable",
            Days = new List<CreateUpdateWorkoutTemplateDayDto>
            {
                new() { Name = "Day 1", Order = 1, Exercises = new List<CreateUpdateWorkoutTemplateExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 3, Reps = "10" } } }
            }
        });

        var plan = await _templateAppService.CloneToTraineeAsync(template.Id, new CloneWorkoutTemplateDto
        {
            TraineeId = trainee.Id
        });

        plan.TraineeId.ShouldBe(trainee.Id);
        plan.IsActive.ShouldBeFalse(); // clones land inactive
        plan.Name.ShouldBe("Cloneable"); // template name used when not overridden
        plan.Days.Single().Exercises.Single().ExerciseId.ShouldBe(ex.Id);

        // The cloned plan is a real, independent plan the coach can now manage.
        var fetched = await _planAppService.GetAsync(plan.Id);
        fetched.Days.Single().Exercises.Single().Sets.ShouldBe(3);
    }

    [Fact]
    public async Task Should_Clone_With_Name_Override()
    {
        var trainee = await CreateTraineeAsync();
        var template = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto { Name = "Base" });

        var plan = await _templateAppService.CloneToTraineeAsync(template.Id, new CloneWorkoutTemplateDto
        {
            TraineeId = trainee.Id,
            Name = "Custom Name"
        });

        plan.Name.ShouldBe("Custom Name");
    }

    [Fact]
    public async Task Should_Throw_When_Cloning_To_Unknown_Trainee()
    {
        var template = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto { Name = "T" });

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _templateAppService.CloneToTraineeAsync(template.Id, new CloneWorkoutTemplateDto { TraineeId = Guid.NewGuid() }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.TraineeNotFound);
    }

    [Fact]
    public async Task Should_Delete_Template()
    {
        var created = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto { Name = "Temp" });

        await _templateAppService.DeleteAsync(created.Id);

        var result = await _templateAppService.GetListAsync(new GetWorkoutPlanTemplateListInput());
        result.Items.ShouldNotContain(x => x.Id == created.Id);
    }

    [Fact]
    public async Task Should_Persist_Template_Day_ScheduledDay()
    {
        var template = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto
        {
            Name = "Weekly Split",
            Days = new List<CreateUpdateWorkoutTemplateDayDto>
            {
                new() { Name = "Scheduled", Order = 1, ScheduledDay = DayOfWeek.Monday },
                new() { Name = "Unscheduled", Order = 2 }
            }
        });

        template.Days.Single(d => d.Name == "Scheduled").ScheduledDay.ShouldBe(DayOfWeek.Monday);
        template.Days.Single(d => d.Name == "Unscheduled").ScheduledDay.ShouldBeNull(); // null round-trips (backward compatible)
    }

    [Fact]
    public async Task Should_Carry_ScheduledDay_Through_SaveAsTemplate_And_Clone()
    {
        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync("Row");

        // A source plan with a day scheduled on Wednesday.
        var plan = await _planAppService.CreateAsync(new CreateUpdateWorkoutPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Source",
            IsActive = true,
            Days = new List<CreateUpdateWorkoutDayDto>
            {
                new()
                {
                    Name = "Pull",
                    Order = 1,
                    ScheduledDay = DayOfWeek.Wednesday,
                    Exercises = new List<CreateUpdateWorkoutExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 3 } }
                }
            }
        });

        // Save the plan as a template — the weekday must survive.
        var template = await _templateAppService.SaveAsTemplateAsync(new SaveWorkoutPlanAsTemplateDto
        {
            WorkoutPlanId = plan.Id,
            Name = "From Plan"
        });
        template.Days.Single().ScheduledDay.ShouldBe(DayOfWeek.Wednesday);

        // Clone the template back onto a trainee — the weekday must reach the new WorkoutDay.
        var cloned = await _templateAppService.CloneToTraineeAsync(template.Id, new CloneWorkoutTemplateDto
        {
            TraineeId = trainee.Id
        });
        cloned.Days.Single().ScheduledDay.ShouldBe(DayOfWeek.Wednesday);
    }

    [Fact]
    public async Task Should_Surface_Cloned_Template_Day_In_Today()
    {
        var sampleDate = new DateTime(2026, 3, 2);

        var trainee = await CreateTraineeAsync();
        var ex = await CreateExerciseAsync("Squat");

        // Template with a day scheduled on the sample date's weekday.
        var template = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto
        {
            Name = "Weekly",
            Days = new List<CreateUpdateWorkoutTemplateDayDto>
            {
                new()
                {
                    Name = "Today's Day",
                    Order = 1,
                    ScheduledDay = sampleDate.DayOfWeek,
                    Exercises = new List<CreateUpdateWorkoutTemplateExerciseDto> { new() { ExerciseId = ex.Id, Order = 1, Sets = 5, Reps = "5" } }
                }
            }
        });

        // Clone onto the trainee and activate it (clones land inactive).
        var plan = await _templateAppService.CloneToTraineeAsync(template.Id, new CloneWorkoutTemplateDto { TraineeId = trainee.Id });
        await _planAppService.SetActiveAsync(plan.Id);

        // Today must surface the cloned scheduled day (previously it was always a rest day).
        var today = GetRequiredService<IMyTodayAppService>();
        MyTodayDto result;
        using (ChangeToTrainee(trainee))
        {
            result = await today.GetAsync(new GetMyTodayInput { Date = sampleDate });
        }

        result.IsRestDay.ShouldBeFalse();
        result.ScheduledWorkoutDays.Count.ShouldBe(1);
        result.ScheduledWorkoutDays.Single().Name.ShouldBe("Today's Day");
    }
}
