using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.ProgressEntries;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own progress.</summary>
public interface IMyProgressAppService : IApplicationService
{
    Task<ProgressEntryDto> CreateAsync(CreateMyProgressEntryDto input);

    Task<PagedResultDto<ProgressEntryDto>> GetListAsync(GetMyProgressListInput input);

    Task<ProgressEntryDto> GetAsync(Guid id);

    Task DeleteAsync(Guid id);
}
