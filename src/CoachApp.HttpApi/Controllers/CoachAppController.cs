using CoachApp.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace CoachApp.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class CoachAppController : AbpControllerBase
{
    protected CoachAppController()
    {
        LocalizationResource = typeof(CoachAppResource);
    }
}
