using System;
using CoachApp.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Entites.Trainees;

/// <summary>
/// Trainee aggregate root. Pragmatic domain model (see the CoachApp conventions):
/// properties are publicly settable for straightforward CRUD, while construction
/// goes through <see cref="Create"/>, which validates the required fields and
/// stamps the identity and tenant. Cross-aggregate rules that need I/O (unique
/// code) are enforced in the application service.
/// </summary>
public class Trainee : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; set; }

    public virtual string Code { get; set; } = string.Empty;

    public virtual string FirstName { get; set; } = string.Empty;

    public virtual string LastName { get; set; } = string.Empty;

    public virtual Gender Gender { get; set; }

    public virtual DateTime? BirthDate { get; set; }

    public virtual string? Email { get; set; }

    public virtual string? PhoneNumber { get; set; }

    public virtual string? Address { get; set; }

    public virtual bool IsActive { get; set; }

    /// <summary>For the ORM only.</summary>
    protected Trainee()
    {
    }

    /// <summary>
    /// Creates a valid trainee: generates identity at the call site, validates the
    /// required fields and field lengths, stamps the tenant and activates the record.
    /// </summary>
    public static Trainee Create(
        Guid id,
        string code,
        string firstName,
        string lastName,
        Gender gender = Gender.Unspecified,
        DateTime? birthDate = null,
        string? email = null,
        string? phoneNumber = null,
        string? address = null,
        Guid? tenantId = null)
    {
        return new Trainee
        {
            Id = id,
            Code = Check.NotNullOrWhiteSpace(code, nameof(code), TraineeConsts.MaxCodeLength),
            FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), TraineeConsts.MaxFirstNameLength),
            LastName = Check.NotNullOrWhiteSpace(lastName, nameof(lastName), TraineeConsts.MaxLastNameLength),
            Gender = gender,
            BirthDate = birthDate,
            Email = Check.Length(email, nameof(email), TraineeConsts.MaxEmailLength),
            PhoneNumber = Check.Length(phoneNumber, nameof(phoneNumber), TraineeConsts.MaxPhoneNumberLength),
            Address = Check.Length(address, nameof(address), TraineeConsts.MaxAddressLength),
            TenantId = tenantId,
            IsActive = true
        };
    }

    public virtual void Activate() => IsActive = true;

    public virtual void Deactivate() => IsActive = false;
}
