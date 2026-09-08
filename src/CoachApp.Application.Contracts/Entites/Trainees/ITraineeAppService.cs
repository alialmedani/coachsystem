using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Trainees;

/// <summary>Coach-facing management of trainee accounts (tenant-scoped).</summary>
public interface ITraineeAppService : IApplicationService
{
    Task<TraineeDto> GetAsync(Guid id);

    Task<PagedResultDto<TraineeDto>> GetListAsync(GetTraineeListInput input);

    /// <summary>Creates the trainee's login (IdentityUser + Trainee role) and profile.</summary>
    Task<TraineeDto> CreateAsync(CreateTraineeDto input);

    Task<TraineeDto> UpdateAsync(Guid id, UpdateTraineeDto input);

    Task DeleteAsync(Guid id);
}
