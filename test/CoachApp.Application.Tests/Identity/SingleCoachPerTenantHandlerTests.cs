using System;
using System.Threading.Tasks;
using CoachApp.Apis;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Identity;

/// <summary>
/// Integration test for the PD1 "one coach per tenant" guard (<see cref="SingleCoachPerTenantHandler"/>).
/// The app has no coach-creation flow, so this exercises the guard the way an out-of-band identity
/// operation would: adding a second user to the <c>Coach</c> role must be rejected.
/// </summary>
public abstract class SingleCoachPerTenantHandlerTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;

    protected SingleCoachPerTenantHandlerTests()
    {
        _userManager = GetRequiredService<IdentityUserManager>();
        _roleManager = GetRequiredService<IdentityRoleManager>();
    }

    [Fact]
    public async Task Should_Allow_The_First_Coach_But_Reject_A_Second()
    {
        await WithUnitOfWorkAsync(async () =>
        {
            if (await _roleManager.FindByNameAsync(CoachAppRoles.Coach) == null)
            {
                (await _roleManager.CreateAsync(new IdentityRole(Guid.NewGuid(), CoachAppRoles.Coach))).Succeeded.ShouldBeTrue();
            }
        });

        var uniq = Guid.NewGuid().ToString("N")[..8];

        // First coach: allowed.
        await WithUnitOfWorkAsync(async () =>
        {
            var coach1 = new IdentityUser(Guid.NewGuid(), "coach1_" + uniq, $"coach1_{uniq}@example.com");
            (await _userManager.CreateAsync(coach1)).Succeeded.ShouldBeTrue();
            (await _userManager.AddToRoleAsync(coach1, CoachAppRoles.Coach)).Succeeded.ShouldBeTrue();
        });

        // Second coach: rejected by the PD1 guard (roll back the unit of work).
        var ex = await Should.ThrowAsync<BusinessException>(() => WithUnitOfWorkAsync(async () =>
        {
            var coach2 = new IdentityUser(Guid.NewGuid(), "coach2_" + uniq, $"coach2_{uniq}@example.com");
            (await _userManager.CreateAsync(coach2)).Succeeded.ShouldBeTrue();
            (await _userManager.AddToRoleAsync(coach2, CoachAppRoles.Coach)).Succeeded.ShouldBeTrue();
        }));

        ex.Code.ShouldBe(CoachAppDomainErrorCodes.SecondCoachNotAllowed);
    }

    [Fact]
    public async Task Should_Not_Interfere_With_Trainee_Provisioning()
    {
        // A trainee is created in the Trainee role, so the guard must not trip.
        var trainee = await CreateTraineeAsync();
        trainee.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Should_Not_Crash_When_Updating_A_User_Whose_Roles_Are_Not_Loaded()
    {
        // Regression: the login / lockout / profile-update paths update the IdentityUser WITHOUT
        // loading its Roles navigation (it is null), which previously threw inside IsInRole and broke
        // authentication. The guard must skip that case cleanly.
        var uniq = Guid.NewGuid().ToString("N")[..8];
        var userId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
            (await _userManager.CreateAsync(new IdentityUser(userId, "plain_" + uniq, $"plain_{uniq}@example.com")))
                .Succeeded.ShouldBeTrue());

        // Re-fetch WITHOUT details (Roles not loaded) and update — mirrors the login stamp update.
        await WithUnitOfWorkAsync(async () =>
        {
            var repo = GetRequiredService<IRepository<IdentityUser, Guid>>();
            var user = await repo.GetAsync(userId, includeDetails: false);
            user.SetPhoneNumber("0700000000", false);
            await repo.UpdateAsync(user, autoSave: true); // must not throw
        });
    }
}
