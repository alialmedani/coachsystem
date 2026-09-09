using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own nutrition days.</summary>
public interface IMyNutritionLogAppService : IApplicationService
{
    Task<NutritionLogDto> CreateAsync(CreateNutritionLogDto input);

    /// <summary>"Log From Plan": creates a log snapshotted from one of the trainee's own nutrition plans.</summary>
    Task<NutritionLogDto> CreateFromPlanAsync(CreateNutritionLogFromPlanDto input);

    Task<PagedResultDto<NutritionLogDto>> GetListAsync(GetMyNutritionLogListInput input);

    Task<NutritionLogDto> GetAsync(Guid id);

    Task<NutritionLogDto> UpdateAsync(Guid id, UpdateNutritionLogDto input);

    Task DeleteAsync(Guid id);
}
