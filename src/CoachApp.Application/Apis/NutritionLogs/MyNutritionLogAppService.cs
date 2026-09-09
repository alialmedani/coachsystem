using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Foods;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.NutritionLogs;

/// <summary>Trainee-facing: the signed-in trainee logs and reads their own nutrition days.</summary>
[Authorize(CoachAppPermissions.Trainee.NutritionLogs.Default)]
public class MyNutritionLogAppService : MyTraineeAppServiceBase, IMyNutritionLogAppService
{
    private readonly IRepository<NutritionLog, Guid> _logRepository;
    private readonly IRepository<Food, Guid> _foodRepository;

    public MyNutritionLogAppService(
        IRepository<NutritionLog, Guid> logRepository,
        IRepository<Food, Guid> foodRepository)
    {
        _logRepository = logRepository;
        _foodRepository = foodRepository;
    }

    [Authorize(CoachAppPermissions.Trainee.NutritionLogs.Create)]
    public virtual async Task<NutritionLogDto> CreateAsync(CreateNutritionLogDto input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        await CheckFoodsExistAsync(input.Entries.Select(e => e.FoodId));

        var log = NutritionLog.Create(
            GuidGenerator.Create(),
            traineeId,
            input.Date,
            input.NutritionPlanId,
            input.Notes,
            CurrentTenant.Id);

        foreach (var entry in input.Entries.OrderBy(e => e.Order))
        {
            log.AddEntry(GuidGenerator.Create(), entry.FoodId, entry.Order, entry.Quantity, entry.Notes);
        }

        await _logRepository.InsertAsync(log, autoSave: true);
        return await MapDetailAsync(log);
    }

    public virtual async Task<PagedResultDto<NutritionLogDto>> GetListAsync(GetMyNutritionLogListInput input)
    {
        var traineeId = await GetCurrentTraineeIdAsync();

        var query = (await _logRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == traineeId)
            .WhereIf(input.FromDate.HasValue, x => x.Date >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue, x => x.Date <= input.ToDate!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(NutritionLog.Date)} desc" : input.Sorting;
        var logs = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<NutritionLogDto>(totalCount, ObjectMapper.Map<List<NutritionLog>, List<NutritionLogDto>>(logs));
    }

    public virtual async Task<NutritionLogDto> GetAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var log = await GetOwnedWithDetailsAsync(id, traineeId);
        return await MapDetailAsync(log);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var traineeId = await GetCurrentTraineeIdAsync();
        var log = await GetOwnedWithDetailsAsync(id, traineeId);
        await _logRepository.DeleteAsync(log, autoSave: true);
    }

    private async Task<NutritionLog> GetOwnedWithDetailsAsync(Guid id, Guid traineeId)
    {
        var query = await _logRepository.WithDetailsAsync(x => x.Entries);
        var log = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id && x.TraineeId == traineeId));
        if (log == null)
        {
            throw new EntityNotFoundException(typeof(NutritionLog), id);
        }

        return log;
    }

    private async Task CheckFoodsExistAsync(IEnumerable<Guid> foodIds)
    {
        var ids = foodIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var found = await _foodRepository.CountAsync(x => ids.Contains(x.Id));
        if (found != ids.Count)
        {
            throw new UserFriendlyException(L["OneOrMoreFoodsDoNotExist"]);
        }
    }

    private async Task<NutritionLogDto> MapDetailAsync(NutritionLog log)
    {
        var dto = ObjectMapper.Map<NutritionLog, NutritionLogDto>(log);
        await NutritionLogEnricher.EnrichAsync(dto, _foodRepository);
        return dto;
    }
}
