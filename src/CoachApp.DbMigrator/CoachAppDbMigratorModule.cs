using CoachApp.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace CoachApp.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(CoachAppEntityFrameworkCoreModule),
    typeof(CoachAppApplicationContractsModule)
)]
public class CoachAppDbMigratorModule : AbpModule
{
}
