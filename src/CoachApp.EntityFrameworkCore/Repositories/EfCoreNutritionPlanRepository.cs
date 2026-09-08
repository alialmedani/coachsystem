using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.NutritionPlans;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CoachApp.EntityFrameworkCore;

/// <summary>
/// Custom repository for the <see cref="NutritionPlan"/> aggregate: overrides
/// <c>WithDetailsAsync</c> so <c>GetAsync(id, includeDetails: true)</c> eager-loads
/// the meal/item graph.
/// </summary>
public class EfCoreNutritionPlanRepository
    : EfCoreRepository<CoachAppDbContext, NutritionPlan, Guid>
{
    public EfCoreNutritionPlanRepository(IDbContextProvider<CoachAppDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<NutritionPlan>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(p => p.Meals)
            .ThenInclude(m => m.Items);
    }
}
