using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Exercises;
using CoachApp.Enums;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace CoachApp.Apis.Exercises;

/// <summary>Integration tests for the coach-facing <see cref="IExerciseAppService"/>.</summary>
public abstract class ExerciseAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IExerciseAppService _exerciseAppService;

    protected ExerciseAppServiceTests()
    {
        _exerciseAppService = GetRequiredService<IExerciseAppService>();
    }

    [Fact]
    public async Task Should_Create_Exercise_When_Input_Is_Valid()
    {
        var result = await _exerciseAppService.CreateAsync(new CreateUpdateExerciseDto
        {
            Name = "Deadlift",
            Description = "Hinge movement",
            TargetMuscle = MuscleGroup.Back,
            Equipment = Equipment.Barbell,
            IsActive = true
        });

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Deadlift");
        result.TargetMuscle.ShouldBe(MuscleGroup.Back);
        result.Equipment.ShouldBe(Equipment.Barbell);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Get_Exercise_By_Id()
    {
        var created = await CreateExerciseAsync("Overhead Press", MuscleGroup.Shoulders);

        var fetched = await _exerciseAppService.GetAsync(created.Id);

        fetched.Id.ShouldBe(created.Id);
        fetched.Name.ShouldBe("Overhead Press");
        fetched.TargetMuscle.ShouldBe(MuscleGroup.Shoulders);
    }

    [Fact]
    public async Task Should_Filter_List_By_Name()
    {
        await CreateExerciseAsync("Barbell Squat", MuscleGroup.Legs);
        await CreateExerciseAsync("Leg Press", MuscleGroup.Legs);
        await CreateExerciseAsync("Bicep Curl", MuscleGroup.Arms);

        var result = await _exerciseAppService.GetListAsync(new GetExerciseListInput { Filter = "Squat" });

        result.TotalCount.ShouldBe(1);
        result.Items.Single().Name.ShouldBe("Barbell Squat");
    }

    [Fact]
    public async Task Should_Filter_List_By_TargetMuscle()
    {
        await CreateExerciseAsync("Barbell Squat", MuscleGroup.Legs);
        await CreateExerciseAsync("Leg Press", MuscleGroup.Legs);
        await CreateExerciseAsync("Bicep Curl", MuscleGroup.Arms);

        var result = await _exerciseAppService.GetListAsync(new GetExerciseListInput { TargetMuscle = MuscleGroup.Legs });

        result.TotalCount.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.TargetMuscle == MuscleGroup.Legs);
    }

    [Fact]
    public async Task Should_Page_The_List()
    {
        for (var i = 0; i < 5; i++)
        {
            await CreateExerciseAsync($"Ex {i:D2}");
        }

        var page = await _exerciseAppService.GetListAsync(new GetExerciseListInput { SkipCount = 0, MaxResultCount = 2 });

        page.TotalCount.ShouldBe(5);
        page.Items.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Should_Update_Exercise()
    {
        var created = await CreateExerciseAsync("Row", MuscleGroup.Back);

        var updated = await _exerciseAppService.UpdateAsync(created.Id, new CreateUpdateExerciseDto
        {
            Name = "Cable Row",
            TargetMuscle = MuscleGroup.Back,
            Equipment = Equipment.Cable,
            IsActive = false
        });

        updated.Name.ShouldBe("Cable Row");
        updated.Equipment.ShouldBe(Equipment.Cable);
        updated.IsActive.ShouldBeFalse();

        var refetched = await _exerciseAppService.GetAsync(created.Id);
        refetched.Name.ShouldBe("Cable Row");
    }

    [Fact]
    public async Task Should_Delete_Exercise()
    {
        var created = await CreateExerciseAsync("Temp");

        await _exerciseAppService.DeleteAsync(created.Id);

        await Should.ThrowAsync<EntityNotFoundException>(() => _exerciseAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Throw_Validation_When_Name_Is_Missing()
    {
        await Should.ThrowAsync<AbpValidationException>(() =>
            _exerciseAppService.CreateAsync(new CreateUpdateExerciseDto { Name = "" }));
    }
}
