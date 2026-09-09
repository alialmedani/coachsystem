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
}
