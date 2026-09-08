using CoachApp.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace CoachApp.Permissions;

public class CoachAppPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(CoachAppPermissions.GroupName, L("Permission:CoachApp"));

        var trainees = group.AddPermission(CoachAppPermissions.Trainees.Default, L("Permission:Trainees"));
        trainees.AddChild(CoachAppPermissions.Trainees.Create, L("Permission:Trainees.Create"));
        trainees.AddChild(CoachAppPermissions.Trainees.Update, L("Permission:Trainees.Update"));
        trainees.AddChild(CoachAppPermissions.Trainees.Delete, L("Permission:Trainees.Delete"));

        var trainingPlans = group.AddPermission(CoachAppPermissions.TrainingPlans.Default, L("Permission:TrainingPlans"));
        trainingPlans.AddChild(CoachAppPermissions.TrainingPlans.Create, L("Permission:TrainingPlans.Create"));
        trainingPlans.AddChild(CoachAppPermissions.TrainingPlans.Update, L("Permission:TrainingPlans.Update"));
        trainingPlans.AddChild(CoachAppPermissions.TrainingPlans.Delete, L("Permission:TrainingPlans.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CoachAppResource>(name);
    }
}
