using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.NutritionLogs;

/// <summary>Coach-facing: read a trainee's logged nutrition days (adherence tracking).</summary>
public interface INutritionLogAppService : IApplicationService
{
    Task<PagedResultDto<NutritionLogDto>> GetListAsync(GetNutritionLogListInput input);

    Task<NutritionLogDto> GetAsync(Guid id);
}
