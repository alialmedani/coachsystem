using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace CoachApp.Training.Trainees;

/// <summary>
/// Domain service for the <see cref="Trainee"/> aggregate. Encapsulates the
/// business rules that need repository access (code uniqueness) and delegates
/// object construction to <see cref="TraineeFactory"/>.
/// </summary>
public class TraineeManager : DomainService
{
    private readonly ITraineeRepository _traineeRepository;
    private readonly TraineeFactory _traineeFactory;

    public TraineeManager(
        ITraineeRepository traineeRepository,
        TraineeFactory traineeFactory)
    {
        _traineeRepository = traineeRepository;
        _traineeFactory = traineeFactory;
    }

    public virtual async Task<Trainee> CreateAsync(
        string code,
        string firstName,
        string lastName,
        Gender gender = Gender.Unspecified,
        DateTime? birthDate = null,
        string? email = null,
        string? phoneNumber = null,
        string? address = null)
    {
        await CheckCodeDuplicationAsync(code);

        return _traineeFactory.Create(
            code,
            firstName,
            lastName,
            gender,
            birthDate,
            email,
            phoneNumber,
            address
        );
    }

    public virtual async Task ChangeCodeAsync(Trainee trainee, string newCode)
    {
        Check.NotNull(trainee, nameof(trainee));

        if (string.Equals(trainee.Code, newCode, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await CheckCodeDuplicationAsync(newCode, trainee.Id);
        trainee.SetCode(newCode);
    }

    protected virtual async Task CheckCodeDuplicationAsync(string code, Guid? excludedId = null)
    {
        var existing = await _traineeRepository.FindByCodeAsync(code);
        if (existing != null && existing.Id != excludedId)
        {
            throw new BusinessException(TrainingErrorCodes.TraineeCodeAlreadyExists)
                .WithData("Code", code);
        }
    }
}
