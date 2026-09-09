using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.TraineeNotes;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.TraineeNotes;

/// <summary>Coach-facing management of a trainee's notes (tenant-scoped).</summary>
[Authorize(CoachAppPermissions.Coach.Notes.Default)]
public class TraineeNoteAppService : CoachAppAppService, ITraineeNoteAppService
{
    private readonly IRepository<TraineeNote, Guid> _noteRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public TraineeNoteAppService(
        IRepository<TraineeNote, Guid> noteRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _noteRepository = noteRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<TraineeNoteDto> GetAsync(Guid id)
    {
        var note = await _noteRepository.GetAsync(id);
        return ObjectMapper.Map<TraineeNote, TraineeNoteDto>(note);
    }

    public virtual async Task<PagedResultDto<TraineeNoteDto>> GetListAsync(GetTraineeNoteListInput input)
    {
        var query = (await _noteRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == input.TraineeId);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(TraineeNote.Date)} desc" : input.Sorting;
        var notes = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<TraineeNoteDto>(totalCount, ObjectMapper.Map<List<TraineeNote>, List<TraineeNoteDto>>(notes));
    }

    [Authorize(CoachAppPermissions.Coach.Notes.Create)]
    public virtual async Task<TraineeNoteDto> CreateAsync(CreateUpdateTraineeNoteDto input)
    {
        await CheckTraineeExistsAsync(input.TraineeId);

        var note = TraineeNote.Create(GuidGenerator.Create(), input.TraineeId, input.Date, input.Text, CurrentTenant.Id);
        await _noteRepository.InsertAsync(note, autoSave: true);
        return ObjectMapper.Map<TraineeNote, TraineeNoteDto>(note);
    }

    [Authorize(CoachAppPermissions.Coach.Notes.Update)]
    public virtual async Task<TraineeNoteDto> UpdateAsync(Guid id, CreateUpdateTraineeNoteDto input)
    {
        var note = await _noteRepository.GetAsync(id);

        if (note.TraineeId != input.TraineeId)
        {
            await CheckTraineeExistsAsync(input.TraineeId);
            note.TraineeId = input.TraineeId;
        }

        note.Date = input.Date;
        note.Text = input.Text;

        await _noteRepository.UpdateAsync(note, autoSave: true);
        return ObjectMapper.Map<TraineeNote, TraineeNoteDto>(note);
    }

    [Authorize(CoachAppPermissions.Coach.Notes.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _noteRepository.DeleteAsync(id);
    }

    private async Task CheckTraineeExistsAsync(Guid traineeId)
    {
        var trainee = await _traineeRepository.FindAsync(traineeId);
        if (trainee == null)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.TraineeNotFound);
        }
    }
}
