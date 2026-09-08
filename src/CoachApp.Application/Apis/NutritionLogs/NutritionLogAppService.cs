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

/// <summary>Coach-facing: read a trainee's logged nutrition days (adherence tracking).</summary>
[Authorize(CoachAppPermissions.Coach.Tracking.Default)]
public class NutritionLogAppService : CoachAppAppService, INutritionLogAppService
{
    private readonly IRepository<NutritionLog, Guid> _logRepository;
    private readonly IRepository<Food, Guid> _foodRepository;

    public NutritionLogAppService(
        IRepository<NutritionLog, Guid> logRepository,
        IRepository<Food, Guid> foodRepository)
    {
        _logRepository = logRepository;
        _foodRepository = foodRepository;
    }

    public virtual async Task<PagedResultDto<NutritionLogDto>> GetListAsync(GetNutritionLogListInput input)
    {
        var query = (await _logRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == input.TraineeId)
            .WhereIf(input.FromDate.HasValue, x => x.Date >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue, x => x.Date <= input.ToDate!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(NutritionLog.Date)} desc" : input.Sorting;
        var logs = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<NutritionLogDto>(totalCount, ObjectMapper.Map<List<NutritionLog>, List<NutritionLogDto>>(logs));
    }

    public virtual async Task<NutritionLogDto> GetAsync(Guid id)
    {
        var query = await _logRepository.WithDetailsAsync(x => x.Entries);
        var log = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        if (log == null)
        {
            throw new EntityNotFoundException(typeof(NutritionLog), id);
        }

        var dto = ObjectMapper.Map<NutritionLog, NutritionLogDto>(log);
        await NutritionLogEnricher.EnrichAsync(dto, _foodRepository);
        return dto;
    }
}
