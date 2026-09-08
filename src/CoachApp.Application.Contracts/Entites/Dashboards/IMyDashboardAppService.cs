using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Dashboards;

/// <summary>Trainee-facing analytics for the signed-in trainee (read-only).</summary>
public interface IMyDashboardAppService : IApplicationService
{
    Task<NutritionAdherenceDto> GetNutritionAdherenceAsync(GetMyNutritionAdherenceInput input);

    Task<WorkoutCompletionDto> GetWorkoutCompletionAsync(GetMyWorkoutCompletionInput input);

    Task<TraineeDashboardDto> GetSummaryAsync(GetMyDashboardInput input);
}
