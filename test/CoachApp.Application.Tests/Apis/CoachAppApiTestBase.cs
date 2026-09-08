using System;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.Foods;
using CoachApp.Entites.Trainees;
using CoachApp.Enums;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace CoachApp.Apis;

/// <summary>
/// Shared base for coach-side application-service integration tests. Provides small
/// helpers that seed prerequisite data (a trainee, an exercise, a food) through the
/// real application services, so each test can stay independent and self-contained.
/// </summary>
public abstract class CoachAppApiTestBase<TStartupModule> : CoachAppApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    protected const string ValidPassword = "Test1234!";

    protected async Task<TraineeDto> CreateTraineeAsync(
        string? firstName = null,
        string? lastName = null,
        TrainingGoal goal = TrainingGoal.General,
        bool isActive = true)
    {
        var traineeAppService = GetRequiredService<ITraineeAppService>();

        var unique = Guid.NewGuid().ToString("N")[..12];

        return await traineeAppService.CreateAsync(new CreateTraineeDto
        {
            UserName = "trainee_" + unique,
            Password = ValidPassword,
            FirstName = firstName ?? "First" + unique,
            LastName = lastName ?? "Last",
            Goal = goal,
            IsActive = isActive
        });
    }

    protected async Task<ExerciseDto> CreateExerciseAsync(
        string? name = null,
        MuscleGroup muscle = MuscleGroup.Chest,
        Equipment equipment = Equipment.Barbell)
    {
        var exerciseAppService = GetRequiredService<IExerciseAppService>();

        return await exerciseAppService.CreateAsync(new CreateUpdateExerciseDto
        {
            Name = name ?? "Exercise_" + Guid.NewGuid().ToString("N")[..8],
            TargetMuscle = muscle,
            Equipment = equipment
        });
    }

    protected async Task<FoodDto> CreateFoodAsync(
        string? name = null,
        decimal calories = 100m,
        decimal proteinG = 10m,
        decimal carbsG = 20m,
        decimal fatG = 5m,
        decimal servingSize = 100m,
        string servingUnit = "g")
    {
        var foodAppService = GetRequiredService<IFoodAppService>();

        return await foodAppService.CreateAsync(new CreateUpdateFoodDto
        {
            Name = name ?? "Food_" + Guid.NewGuid().ToString("N")[..8],
            ServingSize = servingSize,
            ServingUnit = servingUnit,
            Calories = calories,
            ProteinG = proteinG,
            CarbsG = carbsG,
            FatG = fatG
        });
    }
}
