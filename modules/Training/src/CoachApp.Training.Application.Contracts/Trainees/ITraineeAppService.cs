using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Training.Trainees;

public interface ITraineeAppService : IApplicationService
{
    Task<TraineeDto> GetAsync(Guid id);

    Task<PagedResultDto<TraineeDto>> GetListAsync(GetTraineeListInput input);

    Task<TraineeDto> CreateAsync(CreateUpdateTraineeDto input);

    Task<TraineeDto> UpdateAsync(Guid id, CreateUpdateTraineeDto input);

    Task DeleteAsync(Guid id);
}
