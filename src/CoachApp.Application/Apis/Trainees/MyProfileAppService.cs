using System;
using System.Threading.Tasks;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace CoachApp.Apis.Trainees;

/// <summary>
/// Trainee-facing: the signed-in trainee reads their own profile. Scoped to the
/// current user via <see cref="Trainee.UserId"/>, so a trainee can only see themselves.
/// </summary>
[Authorize(CoachAppPermissions.Trainee.MyProfile.Default)]
public class MyProfileAppService : CoachAppAppService, IMyProfileAppService
{
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public MyProfileAppService(IRepository<Trainee, Guid> traineeRepository)
    {
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<TraineeDto> GetAsync()
    {
        var userId = CurrentUser.GetId();

        var trainee = await _traineeRepository.FirstOrDefaultAsync(x => x.UserId == userId);
        if (trainee == null)
        {
            throw new EntityNotFoundException(typeof(Trainee), userId);
        }

        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }
}
