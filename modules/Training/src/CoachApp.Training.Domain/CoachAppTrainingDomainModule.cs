using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace CoachApp.Training;

[DependsOn(
    typeof(CoachAppTrainingDomainSharedModule),
    typeof(AbpDddDomainModule)
)]
public class CoachAppTrainingDomainModule : AbpModule
{
}
