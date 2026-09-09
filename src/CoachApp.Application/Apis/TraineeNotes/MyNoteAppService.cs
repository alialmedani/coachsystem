using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.TraineeNotes;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.TraineeNotes;

/// <summary>Trainee-facing: the signed-in trainee reads notes their coach wrote for them.</summary>
[Authorize(CoachAppPermissions.Trainee.MyNotes.Default)]
public class MyNoteAppService : MyTraineeAppServiceBase, IMyNoteAppService
{
    private readonly IRepository<TraineeNote, Guid> _noteRepository;

    public MyNoteAppService(
        IRepository<TraineeNote, Guid> noteRepository)
    {
        _noteRepository = noteRepository;
    }

    public virtual async Task<PagedResultDto<TraineeNoteDto>> GetListAsync(GetMyNoteListInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        var query = (await _noteRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == traineeId);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(TraineeNote.Date)} desc" : input.Sorting;
        var notes = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<TraineeNoteDto>(totalCount, ObjectMapper.Map<List<TraineeNote>, List<TraineeNoteDto>>(notes));
    }

    public virtual async Task<TraineeNoteDto> GetAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var note = await _noteRepository.GetAsync(id);
        if (note.TraineeId != traineeId)
        {
            throw new EntityNotFoundException(typeof(TraineeNote), id);
        }

        return ObjectMapper.Map<TraineeNote, TraineeNoteDto>(note);
    }
}
