using CoachApp.Entites.Trainees;
using CoachApp.Entites.TrainingPlans;
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

        // ── TrainingPlans ─────────────────────────────────────────────────────
        TypeAdapterConfig<TrainingPlan, TrainingPlanDto>.NewConfig();
    }
}
