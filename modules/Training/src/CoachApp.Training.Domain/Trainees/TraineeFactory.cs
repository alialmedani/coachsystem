using System;
using Volo.Abp.Domain.Services;

namespace CoachApp.Training.Trainees;

/// <summary>
/// Factory responsible purely for constructing a valid <see cref="Trainee"/>:
/// it generates the identity, stamps the current tenant and enforces the entity
/// invariants. Cross-aggregate rules (e.g. unique code) live in
/// <see cref="TraineeManager"/>, which orchestrates this factory.
/// </summary>
public class TraineeFactory : DomainService
{
    public virtual Trainee Create(
        string code,
        string firstName,
        string lastName,
        Gender gender = Gender.Unspecified,
        DateTime? birthDate = null,
        string? email = null,
        string? phoneNumber = null,
        string? address = null)
    {
        return new Trainee(
            GuidGenerator.Create(),
            code,
            firstName,
            lastName,
            gender,
            birthDate,
            email,
            phoneNumber,
            address,
            CurrentTenant.Id
        );
    }
}
