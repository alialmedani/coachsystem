using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Dashboards;

/// <summary>Coach-facing analytics for a given trainee (read-only).</summary>
public interface ITraineeDashboardAppService : IApplicationService
{
    Task<NutritionAdherenceDto> GetNutritionAdherenceAsync(GetNutritionAdherenceInput input);

    Task<WorkoutCompletionDto> GetWorkoutCompletionAsync(GetWorkoutCompletionInput input);

    Task<TraineeDashboardDto> GetSummaryAsync(GetTraineeDashboardInput input);
}
