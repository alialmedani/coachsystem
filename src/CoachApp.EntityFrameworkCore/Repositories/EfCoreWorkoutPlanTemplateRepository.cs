using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.WorkoutPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CoachApp.EntityFrameworkCore;

/// <summary>
/// Custom repository for the <see cref="WorkoutPlanTemplate"/> aggregate: overrides
/// <c>WithDetailsAsync</c> so that <c>GetAsync(id, includeDetails: true)</c> eager-loads
/// the day/exercise graph.
/// </summary>
public class EfCoreWorkoutPlanTemplateRepository
    : EfCoreRepository<CoachAppDbContext, WorkoutPlanTemplate, Guid>
{
    public EfCoreWorkoutPlanTemplateRepository(IDbContextProvider<CoachAppDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<WorkoutPlanTemplate>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(p => p.Days)
            .ThenInclude(d => d.Exercises);
    }
}
