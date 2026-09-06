using Volo.Abp.Modularity;

namespace CoachApp;

/* Inherit from this class for your domain layer tests. */
public abstract class CoachAppDomainTestBase<TStartupModule> : CoachAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
