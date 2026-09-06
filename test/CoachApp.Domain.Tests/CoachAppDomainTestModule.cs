using Volo.Abp.Modularity;

namespace CoachApp;

[DependsOn(
    typeof(CoachAppDomainModule),
    typeof(CoachAppTestBaseModule)
)]
public class CoachAppDomainTestModule : AbpModule
{

}
