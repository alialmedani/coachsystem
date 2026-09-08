using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.NutritionPlans;

/// <summary>Trainee-facing: the signed-in trainee reads their own nutrition plans.</summary>
public interface IMyNutritionPlanAppService : IApplicationService
{
    Task<List<NutritionPlanDto>> GetListAsync();

    Task<NutritionPlanDto> GetAsync(Guid id);

    Task<NutritionPlanDto?> GetActiveAsync();
}
