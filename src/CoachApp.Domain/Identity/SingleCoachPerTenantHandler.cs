using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.Identity;

namespace CoachApp.Identity;

/// <summary>
/// PD1 — one coach per tenant. After an <see cref="IdentityUser"/> is created or updated with the
/// <c>Coach</c> role, if the current tenant now has more than one coach the operation is rejected:
/// the <see cref="BusinessException"/> propagates out of the unit of work and rolls the change back,
/// so a second coach can never be committed.
///
/// The app itself has no coach-creation flow (trainees are provisioned in the <c>Trainee</c> role and
/// the coach is the tenant's own admin), so this is a defensive guard against a second coach being
/// added out-of-band via ABP identity management. It short-circuits when the <c>Coach</c> role is not
/// provisioned or when the changed user is not a coach, so seeding, trainee provisioning, and ordinary
/// profile/lockout updates are unaffected and never pay for a role query.
/// </summary>
public class SingleCoachPerTenantHandler :
    ILocalEventHandler<EntityCreatedEventData<IdentityUser>>,
    ILocalEventHandler<EntityUpdatedEventData<IdentityUser>>,
    ITransientDependency
{
    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;

    public SingleCoachPerTenantHandler(IdentityUserManager userManager, IdentityRoleManager roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public Task HandleEventAsync(EntityCreatedEventData<IdentityUser> eventData)
        => EnsureAtMostOneCoachAsync(eventData.Entity);

    public Task HandleEventAsync(EntityUpdatedEventData<IdentityUser> eventData)
        => EnsureAtMostOneCoachAsync(eventData.Entity);

    private async Task EnsureAtMostOneCoachAsync(IdentityUser user)
    {
        // The Roles navigation is only loaded for role-change operations. On the hot paths that
        // do NOT change roles (login updates the security stamp / access-failed count, lockout
        // toggles, profile edits) it is null — skip immediately so we neither crash on IsInRole
        // nor run a needless query. This also correctly scopes enforcement to actual role changes.
        if (user.Roles == null)
        {
            return;
        }

        // Skip when the Coach role isn't provisioned in this context (e.g. host seeding before roles exist).
        var coachRole = await _roleManager.FindByNameAsync(CoachAppRoles.Coach);
        if (coachRole == null)
        {
            return;
        }

        // Only role changes that make THIS user a coach matter.
        if (!user.IsInRole(coachRole.Id))
        {
            return;
        }

        // Scoped to the current tenant by ABP's data filter. Count > 1 means the change just produced
        // a second coach for this tenant, which PD1 forbids.
        var coaches = await _userManager.GetUsersInRoleAsync(CoachAppRoles.Coach);
        if (coaches.Count > 1)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.SecondCoachNotAllowed);
        }
    }
}
