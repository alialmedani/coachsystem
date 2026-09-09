using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.ProgressEntries;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.ProgressEntries;

/// <summary>Coach-facing management of a trainee's progress entries (tenant-scoped).</summary>
[Authorize(CoachAppPermissions.Coach.Progress.Default)]
public class ProgressEntryAppService : CoachAppAppService, IProgressEntryAppService
{
    private readonly IRepository<ProgressEntry, Guid> _progressRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public ProgressEntryAppService(
        IRepository<ProgressEntry, Guid> progressRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _progressRepository = progressRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<ProgressEntryDto> GetAsync(Guid id)
    {
        var entry = await _progressRepository.GetAsync(id);
        return ObjectMapper.Map<ProgressEntry, ProgressEntryDto>(entry);
    }

    public virtual async Task<PagedResultDto<ProgressEntryDto>> GetListAsync(GetProgressEntryListInput input)
    {
        var query = (await _progressRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == input.TraineeId)
            .WhereIf(input.FromDate.HasValue, x => x.Date >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue, x => x.Date <= input.ToDate!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(ProgressEntry.Date)} desc" : input.Sorting;
        var entries = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<ProgressEntryDto>(totalCount, ObjectMapper.Map<List<ProgressEntry>, List<ProgressEntryDto>>(entries));
    }

    [Authorize(CoachAppPermissions.Coach.Progress.Create)]
    public virtual async Task<ProgressEntryDto> CreateAsync(CreateUpdateProgressEntryDto input)
    {
        await CheckTraineeExistsAsync(input.TraineeId);

        var entry = ProgressEntry.Create(
            GuidGenerator.Create(), input.TraineeId, input.Date,
            input.WeightKg, input.BodyFatPercent, input.ChestCm, input.WaistCm,
            input.HipsCm, input.ArmCm, input.ThighCm, input.Notes, CurrentTenant.Id);

        await _progressRepository.InsertAsync(entry, autoSave: true);
        return ObjectMapper.Map<ProgressEntry, ProgressEntryDto>(entry);
    }

    [Authorize(CoachAppPermissions.Coach.Progress.Update)]
    public virtual async Task<ProgressEntryDto> UpdateAsync(Guid id, CreateUpdateProgressEntryDto input)
    {
        var entry = await _progressRepository.GetAsync(id);

        if (entry.TraineeId != input.TraineeId)
        {
            await CheckTraineeExistsAsync(input.TraineeId);
            entry.TraineeId = input.TraineeId;
        }

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
        return ObjectMapper.Map<ProgressEntry, ProgressEntryDto>(entry);
    }

    [Authorize(CoachAppPermissions.Coach.Progress.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _progressRepository.DeleteAsync(id);
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
