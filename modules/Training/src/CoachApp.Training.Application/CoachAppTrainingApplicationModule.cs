using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace CoachApp.Training;

[DependsOn(
    typeof(CoachAppTrainingDomainModule),
    typeof(CoachAppTrainingApplicationContractsModule),
    typeof(AbpDddApplicationModule),
    typeof(AbpMapperlyModule)
)]
public class CoachAppTrainingApplicationModule : AbpModule
{
}
