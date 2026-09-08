using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.TraineeNotes;

/// <summary>Trainee-facing: the signed-in trainee reads notes their coach wrote for them.</summary>
public interface IMyNoteAppService : IApplicationService
{
    Task<PagedResultDto<TraineeNoteDto>> GetListAsync(GetMyNoteListInput input);

    Task<TraineeNoteDto> GetAsync(Guid id);
}
