using CoachApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace CoachApp.Permissions;

public class CoachAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(CoachAppPermissions.GroupName, L("Permission:CoachApp"));

        // ── Coach (management) ────────────────────────────────────────────────
        AddCrud(group, CoachAppPermissions.Coach.Trainees.Default, "Coach.Trainees");
        AddCrud(group, CoachAppPermissions.Coach.Exercises.Default, "Coach.Exercises");
        AddCrud(group, CoachAppPermissions.Coach.WorkoutPlans.Default, "Coach.WorkoutPlans");
        AddCrud(group, CoachAppPermissions.Coach.WorkoutPlanTemplates.Default, "Coach.WorkoutPlanTemplates");
        AddCrud(group, CoachAppPermissions.Coach.Foods.Default, "Coach.Foods");
        AddCrud(group, CoachAppPermissions.Coach.NutritionPlans.Default, "Coach.NutritionPlans");
        AddCrud(group, CoachAppPermissions.Coach.NutritionPlanTemplates.Default, "Coach.NutritionPlanTemplates");
        group.AddPermission(CoachAppPermissions.Coach.Tracking.Default, L("Permission:Coach.Tracking"));
        AddCrud(group, CoachAppPermissions.Coach.Progress.Default, "Coach.Progress");
        AddCrud(group, CoachAppPermissions.Coach.Notes.Default, "Coach.Notes");

        // ── Trainee (self-service) ────────────────────────────────────────────
        group.AddPermission(CoachAppPermissions.Trainee.MyProfile.Default, L("Permission:Trainee.MyProfile"));
        group.AddPermission(CoachAppPermissions.Trainee.MyDashboard.Default, L("Permission:Trainee.MyDashboard"));
        group.AddPermission(CoachAppPermissions.Trainee.MyWorkoutPlans.Default, L("Permission:Trainee.MyWorkoutPlans"));

        var workoutLogs = group.AddPermission(CoachAppPermissions.Trainee.WorkoutLogs.Default, L("Permission:Trainee.WorkoutLogs"));
        workoutLogs.AddChild(CoachAppPermissions.Trainee.WorkoutLogs.Create, L("Permission:Trainee.WorkoutLogs.Create"));

        group.AddPermission(CoachAppPermissions.Trainee.MyNutritionPlans.Default, L("Permission:Trainee.MyNutritionPlans"));

        var nutritionLogs = group.AddPermission(CoachAppPermissions.Trainee.NutritionLogs.Default, L("Permission:Trainee.NutritionLogs"));
        nutritionLogs.AddChild(CoachAppPermissions.Trainee.NutritionLogs.Create, L("Permission:Trainee.NutritionLogs.Create"));

        var myProgress = group.AddPermission(CoachAppPermissions.Trainee.MyProgress.Default, L("Permission:Trainee.MyProgress"));
        myProgress.AddChild(CoachAppPermissions.Trainee.MyProgress.Create, L("Permission:Trainee.MyProgress.Create"));

        group.AddPermission(CoachAppPermissions.Trainee.MyNotes.Default, L("Permission:Trainee.MyNotes"));
    }

    private static void AddCrud(PermissionGroupDefinition group, string defaultName, string localizationKey)
    {
        var permission = group.AddPermission(defaultName, L($"Permission:{localizationKey}"));
        permission.AddChild(defaultName + ".Create", L($"Permission:{localizationKey}.Create"));
        permission.AddChild(defaultName + ".Update", L($"Permission:{localizationKey}.Update"));
        permission.AddChild(defaultName + ".Delete", L($"Permission:{localizationKey}.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CoachAppResource>(name);
    }
}
