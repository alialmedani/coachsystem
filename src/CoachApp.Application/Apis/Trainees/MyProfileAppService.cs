using System.Threading.Tasks;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace CoachApp.Apis.Trainees;

/// <summary>
/// Trainee-facing: the signed-in trainee reads their own profile. Scoped to the
/// current user via <see cref="Trainee.UserId"/>, so a trainee can only see themselves.
/// </summary>
[Authorize(CoachAppPermissions.Trainee.MyProfile.Default)]
public class MyProfileAppService : MyTraineeAppServiceBase, IMyProfileAppService
{
    public virtual async Task<TraineeDto> GetAsync()
    {
        var trainee = await GetCurrentTraineeAsync();
        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    /// <summary>
    /// PD6: restricted self-edit. Only contact details and birthdate are updated; the user name is
    /// immutable and coaching fields (goal, height, start/target weight, active state) are left
    /// untouched so they stay coach-owned. "Current weight" is recorded separately as a progress entry.
    /// </summary>
    public virtual async Task<TraineeDto> UpdateAsync(UpdateMyProfileDto input)
    {
        var trainee = await GetCurrentTraineeAsync();

        trainee.PhoneNumber = input.PhoneNumber;
        trainee.Email = input.Email;
        trainee.BirthDate = input.BirthDate;

        await TraineeRepository.UpdateAsync(trainee, autoSave: true);
        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }
}
