using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace CoachApp.Training;

[DependsOn(
    typeof(CoachAppTrainingDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
)]
public class CoachAppTrainingApplicationContractsModule : AbpModule
{
}
