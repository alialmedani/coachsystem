using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Entites.NutritionPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace CoachApp.EntityFrameworkCore;

/// <summary>
/// Custom repository for the <see cref="NutritionPlanTemplate"/> aggregate: overrides
/// <c>WithDetailsAsync</c> so that <c>GetAsync(id, includeDetails: true)</c> eager-loads
/// the meal/item graph.
/// </summary>
public class EfCoreNutritionPlanTemplateRepository
    : EfCoreRepository<CoachAppDbContext, NutritionPlanTemplate, Guid>
{
    public EfCoreNutritionPlanTemplateRepository(IDbContextProvider<CoachAppDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public override async Task<IQueryable<NutritionPlanTemplate>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(p => p.Meals)
            .ThenInclude(m => m.Items);
    }
}
