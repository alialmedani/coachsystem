using System;
using CoachApp.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.Trainees;

/// <summary>
/// Trainee coaching profile. Each trainee is created by their coach, who also
/// provisions the login: <see cref="UserId"/> links this profile 1:1 to an ABP
/// <c>IdentityUser</c> (role "Trainee") in the coach's tenant. Identity/credentials
/// live on the user; coaching data (goal, body metrics) lives here. Tenant-scoped,
/// so ABP's tenant filter isolates each coach's trainees automatically.
/// </summary>
public class Trainee : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    /// <summary>The linked <c>IdentityUser</c> that owns this trainee's login.</summary>
    public virtual Guid UserId { get; set; }

    /// <summary>Snapshot of the login user name (immutable for a trainee).</summary>
    public virtual string UserName { get; set; } = string.Empty;

    public virtual string FirstName { get; set; } = string.Empty;

    public virtual string LastName { get; set; } = string.Empty;

    public virtual string? Email { get; set; }

    public virtual string? PhoneNumber { get; set; }

    public virtual Gender Gender { get; set; }

    public virtual DateTime? BirthDate { get; set; }

    public virtual TrainingGoal Goal { get; set; }

    public virtual decimal? HeightCm { get; set; }

    public virtual decimal? StartWeightKg { get; set; }

    public virtual decimal? TargetWeightKg { get; set; }

    public virtual bool IsActive { get; set; }

    /// <summary>For the ORM only.</summary>
    protected Trainee()
    {
    }

    /// <summary>
    /// Creates a trainee profile bound to an already-created identity user. The
    /// application service creates the <c>IdentityUser</c> first, then calls this
    /// with the resulting <paramref name="userId"/>.
    /// </summary>
    public static Trainee Create(
        Guid id,
        Guid userId,
        string userName,
        string firstName,
        string lastName,
        Gender gender = Gender.Unspecified,
        DateTime? birthDate = null,
        string? email = null,
        string? phoneNumber = null,
        TrainingGoal goal = TrainingGoal.General,
        decimal? heightCm = null,
        decimal? startWeightKg = null,
        decimal? targetWeightKg = null,
        Guid? tenantId = null)
    {
        return new Trainee
        {
            Id = id,
            UserId = userId,
            UserName = Check.NotNullOrWhiteSpace(userName, nameof(userName), TraineeConsts.MaxUserNameLength),
            FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), TraineeConsts.MaxFirstNameLength),
            LastName = Check.NotNullOrWhiteSpace(lastName, nameof(lastName), TraineeConsts.MaxLastNameLength),
            Gender = gender,
            BirthDate = birthDate,
            Email = Check.Length(email, nameof(email), TraineeConsts.MaxEmailLength),
            PhoneNumber = Check.Length(phoneNumber, nameof(phoneNumber), TraineeConsts.MaxPhoneNumberLength),
            Goal = goal,
            HeightCm = heightCm,
            StartWeightKg = startWeightKg,
            TargetWeightKg = targetWeightKg,
            TenantId = tenantId,
            IsActive = true
        };
    }

    public virtual void Activate() => IsActive = true;

    public virtual void Deactivate() => IsActive = false;
}
