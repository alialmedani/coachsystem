using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.WorkoutPlans;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CoachApp.EntityFrameworkCore;

/// <summary>
/// Custom repository for the <see cref="WorkoutPlan"/> aggregate: overrides
/// <c>WithDetailsAsync</c> so that <c>GetAsync(id, includeDetails: true)</c> eager-loads
/// the day/exercise graph (there is no built-in nested-include option).
/// </summary>
public class EfCoreWorkoutPlanRepository
    : EfCoreRepository<CoachAppDbContext, WorkoutPlan, Guid>
{
    public EfCoreWorkoutPlanRepository(IDbContextProvider<CoachAppDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<WorkoutPlan>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(p => p.Days)
            .ThenInclude(d => d.Exercises);
    }
}
