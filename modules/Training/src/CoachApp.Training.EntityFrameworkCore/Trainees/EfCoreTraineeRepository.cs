using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using CoachApp.Training.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CoachApp.Training.Trainees;

public class EfCoreTraineeRepository
    : EfCoreRepository<ITrainingDbContext, Trainee, Guid>, ITraineeRepository
{
    public EfCoreTraineeRepository(IDbContextProvider<ITrainingDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<Trainee?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(
            x => x.Code == code,
            GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<Trainee>> GetListAsync(
        string? filterText = null,
        Gender? gender = null,
        bool? isActive = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = await GetFilteredQueryAsync(filterText, gender, isActive);

        query = query.OrderBy(string.IsNullOrWhiteSpace(sorting)
            ? $"{nameof(Trainee.Code)} asc"
            : sorting);

        return await query
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountAsync(
        string? filterText = null,
        Gender? gender = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = await GetFilteredQueryAsync(filterText, gender, isActive);
        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    protected virtual async Task<IQueryable<Trainee>> GetFilteredQueryAsync(
        string? filterText,
        Gender? gender,
        bool? isActive)
    {
        var dbSet = await GetDbSetAsync();

        return dbSet
            .WhereIf(
                !string.IsNullOrWhiteSpace(filterText),
                x => x.Code.Contains(filterText!)
                     || x.FirstName.Contains(filterText!)
                     || x.LastName.Contains(filterText!))
            .WhereIf(gender.HasValue, x => x.Gender == gender!.Value)
            .WhereIf(isActive.HasValue, x => x.IsActive == isActive!.Value);
    }
}
