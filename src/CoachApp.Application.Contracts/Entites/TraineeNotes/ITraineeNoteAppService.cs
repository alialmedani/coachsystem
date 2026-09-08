using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.TraineeNotes;

/// <summary>Coach-facing management of a trainee's notes (tenant-scoped).</summary>
public interface ITraineeNoteAppService : IApplicationService
{
    Task<TraineeNoteDto> GetAsync(Guid id);

    Task<PagedResultDto<TraineeNoteDto>> GetListAsync(GetTraineeNoteListInput input);

    Task<TraineeNoteDto> CreateAsync(CreateUpdateTraineeNoteDto input);

    Task<TraineeNoteDto> UpdateAsync(Guid id, CreateUpdateTraineeNoteDto input);

    Task DeleteAsync(Guid id);
}
