using System.Reflection;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.ObjectMapping;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace CoachApp;

[DependsOn(
    typeof(CoachAppDomainModule),
    typeof(CoachAppApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpTenantManagementApplicationModule),
    typeof(AbpSettingManagementApplicationModule)
    )]
public class CoachAppApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Build the central Mapster mapping config.
        CoachAppMapsterConfig.Configure();

        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(Assembly.GetExecutingAssembly());

        context.Services.AddSingleton(config);
        context.Services.AddScoped<IMapper, ServiceMapper>();

        // Make Mapster the app's object mapper. RemoveAll guarantees Mapster is the
        // sole IAutoObjectMappingProvider even while the (soon-retired) Mapperly
        // module is still referenced during the migration.
        context.Services.RemoveAll<IAutoObjectMappingProvider>();
        context.Services.AddTransient<IAutoObjectMappingProvider, MapsterObjectMapper>();
    }
}
