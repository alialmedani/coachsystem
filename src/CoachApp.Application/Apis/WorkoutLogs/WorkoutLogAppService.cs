using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.WorkoutLogs;

/// <summary>Coach-facing: read a trainee's logged sessions (progress/adherence tracking).</summary>
[Authorize(CoachAppPermissions.Coach.Tracking.Default)]
public class WorkoutLogAppService : CoachAppAppService, IWorkoutLogAppService
{
    private readonly IRepository<WorkoutLog, Guid> _logRepository;
    private readonly IRepository<Exercise, Guid> _exerciseRepository;

    public WorkoutLogAppService(
        IRepository<WorkoutLog, Guid> logRepository,
        IRepository<Exercise, Guid> exerciseRepository)
    {
        _logRepository = logRepository;
        _exerciseRepository = exerciseRepository;
    }

    public virtual async Task<PagedResultDto<WorkoutLogDto>> GetListAsync(GetWorkoutLogListInput input)
    {
        var query = (await _logRepository.GetQueryableAsync())
            .Where(x => x.TraineeId == input.TraineeId)
            .WhereIf(input.FromDate.HasValue, x => x.Date >= input.FromDate!.Value)
            .WhereIf(input.ToDate.HasValue, x => x.Date <= input.ToDate!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? $"{nameof(WorkoutLog.Date)} desc" : input.Sorting;
        var logs = await AsyncExecuter.ToListAsync(query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<WorkoutLogDto>(totalCount, ObjectMapper.Map<List<WorkoutLog>, List<WorkoutLogDto>>(logs));
    }

    public virtual async Task<WorkoutLogDto> GetAsync(Guid id)
    {
        var query = await _logRepository.WithDetailsAsync(x => x.Entries);
        var log = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Id == id));
        if (log == null)
        {
            throw new EntityNotFoundException(typeof(WorkoutLog), id);
        }

        var dto = ObjectMapper.Map<WorkoutLog, WorkoutLogDto>(log);
        await WorkoutLogEnricher.EnrichAsync(dto, _exerciseRepository);
        return dto;
    }
}
