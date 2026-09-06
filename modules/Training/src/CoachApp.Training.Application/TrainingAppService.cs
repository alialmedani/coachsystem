using CoachApp.Training.Localization;
using Volo.Abp.Application.Services;

namespace CoachApp.Training;

/// <summary>
/// Base class for all Training application services. Wires the module's
/// localization resource so validation/authorization messages resolve here.
/// </summary>
public abstract class TrainingAppService : ApplicationService
{
    protected TrainingAppService()
    {
        LocalizationResource = typeof(TrainingResource);
    }
}
