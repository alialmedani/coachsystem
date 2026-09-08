using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Foods;

public interface IFoodAppService : IApplicationService
{
    Task<FoodDto> GetAsync(Guid id);

    Task<PagedResultDto<FoodDto>> GetListAsync(GetFoodListInput input);

    Task<FoodDto> CreateAsync(CreateUpdateFoodDto input);

    Task<FoodDto> UpdateAsync(Guid id, CreateUpdateFoodDto input);

    Task DeleteAsync(Guid id);
}
