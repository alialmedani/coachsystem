using CoachApp.Entites.Exercises;
using CoachApp.Entites.Foods;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using Mapster;

namespace CoachApp;

/// <summary>
/// Central Mapster configuration for the application layer.
/// Add one <c>TypeAdapterConfig&lt;TEntity, TDto&gt;.NewConfig()</c> line per mapping,
/// grouped by feature. Use <c>.Ignore(...)</c> for properties the app service fills
/// manually and <c>.Map(dest =&gt; ..., src =&gt; ...)</c> for name/shape mismatches.
/// Entity -> DTO only; Create/Update flow through the aggregate's behavior methods.
/// </summary>
public static class CoachAppMapsterConfig
{
    public static void Configure()
    {
        // ── Trainees ──────────────────────────────────────────────────────────
        TypeAdapterConfig<Trainee, TraineeDto>.NewConfig();

        // ── Exercises ─────────────────────────────────────────────────────────
        TypeAdapterConfig<Exercise, ExerciseDto>.NewConfig();

        // ── Foods ─────────────────────────────────────────────────────────────
        TypeAdapterConfig<Food, FoodDto>.NewConfig();

        // ── WorkoutPlans (nested; ExerciseName enriched in the app service) ─────
        TypeAdapterConfig<WorkoutPlan, WorkoutPlanDto>.NewConfig();
        TypeAdapterConfig<WorkoutDay, WorkoutDayDto>.NewConfig();
        TypeAdapterConfig<WorkoutExercise, WorkoutExerciseDto>.NewConfig();

        // ── WorkoutLogs (ExerciseName enriched in the app service) ──────────────
        TypeAdapterConfig<WorkoutLog, WorkoutLogDto>.NewConfig();
        TypeAdapterConfig<WorkoutLogEntry, WorkoutLogEntryDto>.NewConfig();
    }
}
