using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace CoachApp.Training.Trainees;

/// <summary>
/// Trainee aggregate root. Rich domain model: all state is protected and can
/// only be changed through behavior methods that enforce the invariants.
/// Construction goes through <see cref="TraineeFactory"/> / <see cref="TraineeManager"/>.
/// </summary>
public class Trainee : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual string Code { get; protected set; } = string.Empty;

    public virtual string FirstName { get; protected set; } = string.Empty;

    public virtual string LastName { get; protected set; } = string.Empty;

    public virtual Gender Gender { get; protected set; }

    public virtual DateTime? BirthDate { get; protected set; }

    public virtual string? Email { get; protected set; }

    public virtual string? PhoneNumber { get; protected set; }

    public virtual string? Address { get; protected set; }

    public virtual bool IsActive { get; protected set; }

    /// <summary>For the ORM only.</summary>
    protected Trainee()
    {
    }

    /// <summary>
    /// Internal so that only the domain layer (factory / manager) can create a
    /// trainee. The application layer must go through <see cref="TraineeManager"/>.
    /// </summary>
    internal Trainee(
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
        : base(id)
    {
        SetCode(code);
        SetName(firstName, lastName);
        Gender = gender;
        BirthDate = birthDate;
        SetEmail(email);
        SetPhoneNumber(phoneNumber);
        SetAddress(address);
        TenantId = tenantId;
        IsActive = true;
    }

    public virtual Trainee SetName(string firstName, string lastName)
    {
        FirstName = Check.NotNullOrWhiteSpace(firstName, nameof(firstName), TraineeConsts.MaxFirstNameLength);
        LastName = Check.NotNullOrWhiteSpace(lastName, nameof(lastName), TraineeConsts.MaxLastNameLength);
        return this;
    }

    /// <summary>
    /// Code uniqueness is a cross-aggregate rule, so it is enforced by
    /// <see cref="TraineeManager"/>. This setter only validates the format and is
    /// therefore internal to the domain layer.
    /// </summary>
    internal Trainee SetCode(string code)
    {
        Code = Check.NotNullOrWhiteSpace(code, nameof(code), TraineeConsts.MaxCodeLength);
        return this;
    }

    public virtual Trainee SetGender(Gender gender)
    {
        Gender = gender;
        return this;
    }

    public virtual Trainee SetBirthDate(DateTime? birthDate)
    {
        BirthDate = birthDate;
        return this;
    }

    public virtual Trainee SetEmail(string? email)
    {
        Email = Check.Length(email, nameof(email), TraineeConsts.MaxEmailLength);
        return this;
    }

    public virtual Trainee SetPhoneNumber(string? phoneNumber)
    {
        PhoneNumber = Check.Length(phoneNumber, nameof(phoneNumber), TraineeConsts.MaxPhoneNumberLength);
        return this;
    }

    public virtual Trainee SetAddress(string? address)
    {
        Address = Check.Length(address, nameof(address), TraineeConsts.MaxAddressLength);
        return this;
    }

    public virtual Trainee Activate()
    {
        IsActive = true;
        return this;
    }

    public virtual Trainee Deactivate()
    {
        IsActive = false;
        return this;
    }
}
