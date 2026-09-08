using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own nutrition days.</summary>
public interface IMyNutritionLogAppService : IApplicationService
{
    Task<NutritionLogDto> CreateAsync(CreateNutritionLogDto input);

    Task<PagedResultDto<NutritionLogDto>> GetListAsync(GetMyNutritionLogListInput input);

    Task<NutritionLogDto> GetAsync(Guid id);

    Task DeleteAsync(Guid id);
}
