using Microsoft.Extensions.Localization;
using CoachApp.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace CoachApp;

[Dependency(ReplaceServices = true)]
public class CoachAppBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<CoachAppResource> _localizer;

    public CoachAppBrandingProvider(IStringLocalizer<CoachAppResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
