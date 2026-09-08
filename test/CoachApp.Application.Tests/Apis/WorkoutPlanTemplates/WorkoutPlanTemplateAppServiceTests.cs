using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
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
        await Should.ThrowAsync<BusinessException>(() =>
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

        await Should.ThrowAsync<BusinessException>(() =>
            _templateAppService.CloneToTraineeAsync(template.Id, new CloneWorkoutTemplateDto { TraineeId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task Should_Delete_Template()
    {
        var created = await _templateAppService.CreateAsync(new CreateUpdateWorkoutPlanTemplateDto { Name = "Temp" });

        await _templateAppService.DeleteAsync(created.Id);

        var result = await _templateAppService.GetListAsync(new GetWorkoutPlanTemplateListInput());
        result.Items.ShouldNotContain(x => x.Id == created.Id);
    }
}
