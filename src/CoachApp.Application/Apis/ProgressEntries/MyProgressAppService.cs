using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.ProgressEntries;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

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
        return ObjectMapper.Map<ProgressEntry, ProgressEntryDto>(entry);
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

        return new PagedResultDto<ProgressEntryDto>(totalCount, ObjectMapper.Map<List<ProgressEntry>, List<ProgressEntryDto>>(entries));
    }

    public virtual async Task<ProgressEntryDto> GetAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var entry = await _progressRepository.GetAsync(id);
        if (entry.TraineeId != traineeId)
        {
            throw new EntityNotFoundException(typeof(ProgressEntry), id);
        }

        return ObjectMapper.Map<ProgressEntry, ProgressEntryDto>(entry);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var entry = await _progressRepository.GetAsync(id);
        if (entry.TraineeId != traineeId)
        {
            throw new EntityNotFoundException(typeof(ProgressEntry), id);
        }

        await _progressRepository.DeleteAsync(entry, autoSave: true);
    }
}
