using CoachApp.Training.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace CoachApp.Training.Permissions;

public class TrainingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var trainingGroup = context.AddGroup(TrainingPermissions.GroupName, L("Permission:Training"));

        var trainees = trainingGroup.AddPermission(TrainingPermissions.Trainees.Default, L("Permission:Trainees"));
        trainees.AddChild(TrainingPermissions.Trainees.Create, L("Permission:Trainees.Create"));
        trainees.AddChild(TrainingPermissions.Trainees.Update, L("Permission:Trainees.Update"));
        trainees.AddChild(TrainingPermissions.Trainees.Delete, L("Permission:Trainees.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TrainingResource>(name);
    }
}
