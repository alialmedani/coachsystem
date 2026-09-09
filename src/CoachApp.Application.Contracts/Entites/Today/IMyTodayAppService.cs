using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Today;

/// <summary>Trainee-facing: the signed-in trainee's "Today" view (workout + nutrition + status).</summary>
public interface IMyTodayAppService : IApplicationService
{
    Task<MyTodayDto> GetAsync(GetMyTodayInput input);
}
