using Volo.Abp.Settings;

namespace CoachApp.Settings;

public class CoachAppSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(CoachAppSettings.MySetting1));
    }
}
