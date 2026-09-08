using System;
using System.Threading.Tasks;
using CoachApp.Entites.WorkoutPlans;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.WorkoutPlanTemplates;

/// <summary>Coach-facing workout plan template library + clone / save-as-template (tenant-scoped).</summary>
public interface IWorkoutPlanTemplateAppService : IApplicationService
{
    /// <summary>Full template with days and exercises.</summary>
    Task<WorkoutPlanTemplateDto> GetAsync(Guid id);

    /// <summary>Paged template summaries (no days loaded).</summary>
    Task<PagedResultDto<WorkoutPlanTemplateDto>> GetListAsync(GetWorkoutPlanTemplateListInput input);

    Task<WorkoutPlanTemplateDto> CreateAsync(CreateUpdateWorkoutPlanTemplateDto input);

    Task<WorkoutPlanTemplateDto> UpdateAsync(Guid id, CreateUpdateWorkoutPlanTemplateDto input);

    Task DeleteAsync(Guid id);

    /// <summary>Clones this template onto a trainee as a new, inactive workout plan.</summary>
    Task<WorkoutPlanDto> CloneToTraineeAsync(Guid id, CloneWorkoutTemplateDto input);

    /// <summary>Snapshots an existing trainee workout plan into a new template.</summary>
    Task<WorkoutPlanTemplateDto> SaveAsTemplateAsync(SaveWorkoutPlanAsTemplateDto input);
}
