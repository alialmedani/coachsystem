using System.Linq;
using System.Threading.Tasks;
using CoachApp.MultiTenancy;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;
using Volo.Abp.TenantManagement;

namespace CoachApp.Data;

/// <summary>
/// Seeds the two CoachApp personas and (for local dev) a demo coach tenant.
/// Runs for the host and for every tenant (ABP invokes seed contributors in each
/// context), so the <c>Coach</c> and <c>Trainee</c> roles + their permission grants
/// exist wherever coaches and trainees live.
/// </summary>
public class CoachAppDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string CoachPermissionPrefix = CoachAppPermissionPrefixes.Coach;
    private const string TraineePermissionPrefix = CoachAppPermissionPrefixes.Trainee;

    private readonly IdentityRoleManager _roleManager;
    private readonly IPermissionManager _permissionManager;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly ITenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;
    private readonly IGuidGenerator _guidGenerator;

    public CoachAppDataSeedContributor(
        IdentityRoleManager roleManager,
        IPermissionManager permissionManager,
        IPermissionDefinitionManager permissionDefinitionManager,
        ICurrentTenant currentTenant,
        ITenantManager tenantManager,
        ITenantRepository tenantRepository,
        IGuidGenerator guidGenerator)
    {
        _roleManager = roleManager;
        _permissionManager = permissionManager;
        _permissionDefinitionManager = permissionDefinitionManager;
        _currentTenant = currentTenant;
        _tenantManager = tenantManager;
        _tenantRepository = tenantRepository;
        _guidGenerator = guidGenerator;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        await EnsureRoleWithPermissionsAsync(CoachAppRoles.Coach, CoachPermissionPrefix);
        await EnsureRoleWithPermissionsAsync(CoachAppRoles.Trainee, TraineePermissionPrefix);

        // Provision a demo coach tenant from the host context so tenant-per-coach
        // is testable out of the box; the tenant's auto-seeded admin acts as the coach.
        if (context.TenantId == null && MultiTenancyConsts.IsEnabled)
        {
            await EnsureDemoTenantAsync();
        }
    }

    private async Task EnsureRoleWithPermissionsAsync(string roleName, string permissionPrefix)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            role = new IdentityRole(_guidGenerator.Create(), roleName, _currentTenant.Id);
            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                throw new AbpException(
                    $"Failed to create role '{roleName}': " +
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        var permissions = (await _permissionDefinitionManager.GetPermissionsAsync())
            .Where(p => p.Name.StartsWith(permissionPrefix + "."))
            .Select(p => p.Name)
            .ToList();

        foreach (var permission in permissions)
        {
            await _permissionManager.SetForRoleAsync(roleName, permission, true);
        }
    }

    private async Task EnsureDemoTenantAsync()
    {
        var existing = await _tenantRepository.FindByNameAsync(CoachAppConsts.DemoCoachTenantName);
        if (existing != null)
        {
            return;
        }

        var tenant = await _tenantManager.CreateAsync(CoachAppConsts.DemoCoachTenantName);
        await _tenantRepository.InsertAsync(tenant, autoSave: true);
    }
}
