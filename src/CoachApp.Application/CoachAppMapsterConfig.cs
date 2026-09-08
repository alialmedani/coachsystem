using CoachApp.Entites.Exercises;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.NutritionPlanTemplates;
using CoachApp.Entites.ProgressEntries;
using CoachApp.Entites.TraineeNotes;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Entites.WorkoutPlanTemplates;
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

        // ── WorkoutPlanTemplates (nested; ExerciseName enriched in the app service) ─
        TypeAdapterConfig<WorkoutPlanTemplate, WorkoutPlanTemplateDto>.NewConfig();
        TypeAdapterConfig<WorkoutTemplateDay, WorkoutTemplateDayDto>.NewConfig();
        TypeAdapterConfig<WorkoutTemplateExercise, WorkoutTemplateExerciseDto>.NewConfig();

        // ── WorkoutLogs (ExerciseName enriched in the app service) ──────────────
        TypeAdapterConfig<WorkoutLog, WorkoutLogDto>.NewConfig();
        TypeAdapterConfig<WorkoutLogEntry, WorkoutLogEntryDto>.NewConfig();

        // ── NutritionPlans (food name + macros enriched in the app service) ─────
        TypeAdapterConfig<NutritionPlan, NutritionPlanDto>.NewConfig();
        TypeAdapterConfig<Meal, MealDto>.NewConfig();
        TypeAdapterConfig<MealItem, MealItemDto>.NewConfig();

        // ── NutritionPlanTemplates (food name + macros enriched in the app service) ─
        TypeAdapterConfig<NutritionPlanTemplate, NutritionPlanTemplateDto>.NewConfig();
        TypeAdapterConfig<NutritionTemplateMeal, NutritionTemplateMealDto>.NewConfig();
        TypeAdapterConfig<NutritionTemplateItem, NutritionTemplateItemDto>.NewConfig();

        // ── NutritionLogs (food name + macros enriched in the app service) ──────
        TypeAdapterConfig<NutritionLog, NutritionLogDto>.NewConfig();
        TypeAdapterConfig<NutritionLogEntry, NutritionLogEntryDto>.NewConfig();

        // ── ProgressEntries ─────────────────────────────────────────────────────
        TypeAdapterConfig<ProgressEntry, ProgressEntryDto>.NewConfig();

        // ── TraineeNotes ────────────────────────────────────────────────────────
        TypeAdapterConfig<TraineeNote, TraineeNoteDto>.NewConfig();
    }
}
