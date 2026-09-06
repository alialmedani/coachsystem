using Volo.Abp.Modularity;

namespace CoachApp;

public abstract class CoachAppApplicationTestBase<TStartupModule> : CoachAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
