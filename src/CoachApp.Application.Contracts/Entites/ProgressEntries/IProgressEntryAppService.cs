using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.ProgressEntries;

/// <summary>Coach-facing management of a trainee's progress entries (tenant-scoped).</summary>
public interface IProgressEntryAppService : IApplicationService
{
    Task<ProgressEntryDto> GetAsync(Guid id);

    Task<PagedResultDto<ProgressEntryDto>> GetListAsync(GetProgressEntryListInput input);

    Task<ProgressEntryDto> CreateAsync(CreateUpdateProgressEntryDto input);

    Task<ProgressEntryDto> UpdateAsync(Guid id, CreateUpdateProgressEntryDto input);

    Task DeleteAsync(Guid id);
}
