using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Trainees;

/// <summary>Trainee-facing: the signed-in trainee reads and (restricted) edits their own profile.</summary>
public interface IMyProfileAppService : IApplicationService
{
    Task<TraineeDto> GetAsync();

    /// <summary>PD6: restricted self-edit of contact details + birthdate only.</summary>
    Task<TraineeDto> UpdateAsync(UpdateMyProfileDto input);
}
