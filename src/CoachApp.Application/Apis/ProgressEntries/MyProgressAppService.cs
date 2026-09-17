using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.ProgressEntries;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace CoachApp.Apis.ProgressEntries;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own progress.</summary>
[Authorize(CoachAppPermissions.Trainee.MyProgress.Default)]
public class MyProgressAppService : MyTraineeAppServiceBase, IMyProgressAppService
{
    private readonly IRepository<ProgressEntry, Guid> _progressRepository;

    public MyProgressAppService(
        IRepository<ProgressEntry, Guid> progressRepository)
    {
        _progressRepository = progressRepository;
    }

    [Authorize(CoachAppPermissions.Trainee.MyProgress.Create)]
    public virtual async Task<ProgressEntryDto> CreateAsync(CreateMyProgressEntryDto input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        var entry = ProgressEntry.Create(
            GuidGenerator.Create(), traineeId, input.Date,
            input.WeightKg, input.BodyFatPercent, input.ChestCm, input.WaistCm,
            input.HipsCm, input.ArmCm, input.ThighCm, input.Notes, CurrentTenant.Id);

        await _progressRepository.InsertAsync(entry, autoSave: true);
        return MapWithAuthor(entry);
    }

    // F3/PD4: the trainee edits one of their OWN entries. Gated by the same permission as Create
    // (authoring-class capability); a coach-authored entry is read-only to the trainee.
    [Authorize(CoachAppPermissions.Trainee.MyProgress.Create)]
    public virtual async Task<ProgressEntryDto> UpdateAsync(Guid id, UpdateMyProgressEntryDto input)
    {
        var entry = await GetOwnEntryAsync(id);
        EnsureSelfAuthored(entry);

        entry.Date = input.Date;
        entry.WeightKg = input.WeightKg;
        entry.BodyFatPercent = input.BodyFatPercent;
        entry.ChestCm = input.ChestCm;
        entry.WaistCm = input.WaistCm;
        entry.HipsCm = input.HipsCm;
        entry.ArmCm = input.ArmCm;
        entry.ThighCm = input.ThighCm;
        entry.Notes = input.Notes;

        await _progressRepository.UpdateAsync(entry, autoSave: true);
        return MapWithAuthor(entry);
    }

    public virtual async Task<PagedResultDto<ProgressEntryDto>> GetListAsync(GetMyProgressListInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        var query = (await _progressRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == traineeId)
            .WhereIf(input.FromDate.HasValue, x => x.Date >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue, x => x.Date <= input.ToDate!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(ProgressEntry.Date)} desc" : input.Sorting;
        var entries = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<ProgressEntryDto>(totalCount, entries.Select(MapWithAuthor).ToList());
    }

    public virtual async Task<ProgressEntryDto> GetAsync(Guid id)
    {
        var entry = await GetOwnEntryAsync(id);
        return MapWithAuthor(entry);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var entry = await GetOwnEntryAsync(id);
        EnsureSelfAuthored(entry);
        await _progressRepository.DeleteAsync(entry, autoSave: true);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    /// <summary>Loads an entry and enforces cross-trainee isolation (foreign id → 404).</summary>
    private async Task<ProgressEntry> GetOwnEntryAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var entry = await _progressRepository.GetAsync(id);
        if (entry.TraineeId != traineeId)
        {
            throw new EntityNotFoundException(typeof(ProgressEntry), id);
        }

        return entry;
    }

    /// <summary>F4/PD4: a trainee may only modify entries they authored themselves.</summary>
    private void EnsureSelfAuthored(ProgressEntry entry)
    {
        if (entry.CreatorId != CurrentUser.GetId())
        {
            throw new BusinessException(CoachAppDomainErrorCodes.CannotModifyCoachAuthoredProgress);
        }
    }

    /// <summary>Maps an entry and stamps <see cref="ProgressEntryDto.IsCoachAuthored"/> from the trainee's view.</summary>
    private ProgressEntryDto MapWithAuthor(ProgressEntry entry)
    {
        var dto = ObjectMapper.Map<ProgressEntry, ProgressEntryDto>(entry);
        dto.IsCoachAuthored = entry.CreatorId != CurrentUser.GetId();
        return dto;
    }
}
