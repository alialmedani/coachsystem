using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Trainees;

/// <summary>Trainee-facing: the signed-in trainee reads their own profile.</summary>
public interface IMyProfileAppService : IApplicationService
{
    Task<TraineeDto> GetAsync();
}
