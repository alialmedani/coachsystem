using Volo.Abp.Modularity;

namespace CoachApp;

[DependsOn(
    typeof(CoachAppApplicationModule),
    typeof(CoachAppDomainTestModule)
)]
public class CoachAppApplicationTestModule : AbpModule
{

}
