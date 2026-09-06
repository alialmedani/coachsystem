using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Training.Trainees;

/// <summary>
/// Custom repository for the <see cref="Trainee"/> aggregate root. Defined only
/// because we need query methods beyond the generic repository (code lookup and
/// filtered/paged listing).
/// </summary>
public interface ITraineeRepository : IRepository<Trainee, Guid>
{
    Task<Trainee?> FindByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<List<Trainee>> GetListAsync(
        string? filterText = null,
        Gender? gender = null,
        bool? isActive = null,
        string? sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    Task<long> GetCountAsync(
        string? filterText = null,
        Gender? gender = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);
}
