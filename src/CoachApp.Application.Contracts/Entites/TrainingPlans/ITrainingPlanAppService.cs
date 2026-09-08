using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.TrainingPlans;

public interface ITrainingPlanAppService : IApplicationService
{
    Task<TrainingPlanDto> GetAsync(Guid id);

    Task<PagedResultDto<TrainingPlanDto>> GetListAsync(GetTrainingPlanListInput input);

    Task<TrainingPlanDto> CreateAsync(CreateUpdateTrainingPlanDto input);

    Task<TrainingPlanDto> UpdateAsync(Guid id, CreateUpdateTrainingPlanDto input);

    Task DeleteAsync(Guid id);
}
