using CoachApp.Localization;
using Volo.Abp.Application.Services;

namespace CoachApp;

/* Inherit your application services from this class.
 */
public abstract class CoachAppAppService : ApplicationService
{
    protected CoachAppAppService()
    {
        LocalizationResource = typeof(CoachAppResource);
    }
}
