using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.NutritionPlans;

/// <summary>Coach-facing management of trainees' nutrition plans (tenant-scoped).</summary>
public interface INutritionPlanAppService : IApplicationService
{
    Task<NutritionPlanDto> GetAsync(Guid id);

    Task<PagedResultDto<NutritionPlanDto>> GetListAsync(GetNutritionPlanListInput input);

    Task<NutritionPlanDto> CreateAsync(CreateUpdateNutritionPlanDto input);

    Task<NutritionPlanDto> UpdateAsync(Guid id, CreateUpdateNutritionPlanDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Makes this the trainee's active plan; deactivates their others.</summary>
    Task<NutritionPlanDto> SetActiveAsync(Guid id);
}
