using System;
using System.Threading.Tasks;
using CoachApp.Entites.Trainees;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace CoachApp;

/// <summary>
/// Base class for trainee self-service ("My...") application services. Centralizes resolving the
/// signed-in user to their <see cref="Trainee"/> record via <see cref="Trainee.UserId"/>, so a
/// trainee only ever operates on their own data. The trainee id is always taken from the current
/// user, never from client input.
/// </summary>
public abstract class MyTraineeAppServiceBase : CoachAppAppService
{
    protected IRepository<Trainee, Guid> TraineeRepository
        => LazyServiceProvider.LazyGetRequiredService<IRepository<Trainee, Guid>>();

    /// <summary>Resolves the current user to their trainee id; throws if there is no trainee profile.</summary>
    protected async Task<Guid> GetCurrentTraineeIdAsync()
    {
        return (await GetCurrentTraineeAsync()).Id;
    }

    /// <summary>Resolves the current user to their <see cref="Trainee"/> entity; throws if none.</summary>
    protected async Task<Trainee> GetCurrentTraineeAsync()
    {
        var userId = CurrentUser.GetId();
        var trainee = await TraineeRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (trainee == null)
        {
            throw new EntityNotFoundException(typeof(Trainee), userId);
        }

        return trainee;
    }
}
