using System;
using System.Threading.Tasks;
using CoachApp.Entites.NutritionPlans;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.NutritionPlanTemplates;

/// <summary>Coach-facing nutrition plan template library + clone / save-as-template (tenant-scoped).</summary>
public interface INutritionPlanTemplateAppService : IApplicationService
{
    /// <summary>Full template with meals and items (macros enriched).</summary>
    Task<NutritionPlanTemplateDto> GetAsync(Guid id);

    /// <summary>Paged template summaries (no meals loaded).</summary>
    Task<PagedResultDto<NutritionPlanTemplateDto>> GetListAsync(GetNutritionPlanTemplateListInput input);

    Task<NutritionPlanTemplateDto> CreateAsync(CreateUpdateNutritionPlanTemplateDto input);

    Task<NutritionPlanTemplateDto> UpdateAsync(Guid id, CreateUpdateNutritionPlanTemplateDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Clones this template onto a trainee as a new, inactive nutrition plan.</summary>
    Task<NutritionPlanDto> CloneToTraineeAsync(Guid id, CloneNutritionTemplateDto input);

    /// <summary>Snapshots an existing trainee nutrition plan into a new template.</summary>
    Task<NutritionPlanTemplateDto> SaveAsTemplateAsync(SaveNutritionPlanAsTemplateDto input);
}
