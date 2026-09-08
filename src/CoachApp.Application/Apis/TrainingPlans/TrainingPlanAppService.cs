using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.TrainingPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.TrainingPlans;

[Authorize(CoachAppPermissions.TrainingPlans.Default)]
public class TrainingPlanAppService : CoachAppAppService, ITrainingPlanAppService
{
    private readonly IRepository<TrainingPlan, Guid> _trainingPlanRepository;
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public TrainingPlanAppService(
        IRepository<TrainingPlan, Guid> trainingPlanRepository,
        IRepository<Trainee, Guid> traineeRepository)
    {
        _trainingPlanRepository = trainingPlanRepository;
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<TrainingPlanDto> GetAsync(Guid id)
    {
        var trainingPlan = await _trainingPlanRepository.GetAsync(id);
        return ObjectMapper.Map<TrainingPlan, TrainingPlanDto>(trainingPlan);
    }

    public virtual async Task<PagedResultDto<TrainingPlanDto>> GetListAsync(GetTrainingPlanListInput input)
    {
        var query = await _trainingPlanRepository.GetQueryableAsync();

        query = query
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                x => x.Title.Contains(input.Filter!)
                     || (x.Description != null && x.Description.Contains(input.Filter!)))
            .WhereIf(input.TraineeId.HasValue, x => x.TraineeId == input.TraineeId!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(TrainingPlan.StartDate)} desc"
            : input.Sorting;

        var trainingPlans = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<TrainingPlanDto>(
            totalCount,
            ObjectMapper.Map<List<TrainingPlan>, List<TrainingPlanDto>>(trainingPlans));
    }

    [Authorize(CoachAppPermissions.TrainingPlans.Create)]
    public virtual async Task<TrainingPlanDto> CreateAsync(CreateUpdateTrainingPlanDto input)
    {
        await CheckTraineeExistsAsync(input.TraineeId);

        var trainingPlan = TrainingPlan.Create(
            GuidGenerator.Create(),
            input.TraineeId,
            input.Title,
            input.StartDate,
            input.EndDate,
            input.Description,
            CurrentTenant.Id);

        trainingPlan.IsActive = input.IsActive;

        await _trainingPlanRepository.InsertAsync(trainingPlan, autoSave: true);

        return ObjectMapper.Map<TrainingPlan, TrainingPlanDto>(trainingPlan);
    }

    [Authorize(CoachAppPermissions.TrainingPlans.Update)]
    public virtual async Task<TrainingPlanDto> UpdateAsync(Guid id, CreateUpdateTrainingPlanDto input)
    {
        var trainingPlan = await _trainingPlanRepository.GetAsync(id);

        if (trainingPlan.TraineeId != input.TraineeId)
        {
            await CheckTraineeExistsAsync(input.TraineeId);
            trainingPlan.TraineeId = input.TraineeId;
        }

        trainingPlan.Title = input.Title;
        trainingPlan.Description = input.Description;
        trainingPlan.SetSchedule(input.StartDate, input.EndDate);
        trainingPlan.IsActive = input.IsActive;

        await _trainingPlanRepository.UpdateAsync(trainingPlan, autoSave: true);

        return ObjectMapper.Map<TrainingPlan, TrainingPlanDto>(trainingPlan);
    }

    [Authorize(CoachAppPermissions.TrainingPlans.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _trainingPlanRepository.DeleteAsync(id);
    }

    /// <summary>
    /// A plan must reference an existing trainee. This is a cross-aggregate rule
    /// that needs repository access, so it lives in the application service.
    /// </summary>
    protected virtual async Task CheckTraineeExistsAsync(Guid traineeId)
    {
        var trainee = await _traineeRepository.FindAsync(traineeId);
        if (trainee == null)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.TrainingPlanTraineeNotFound)
                .WithData("TraineeId", traineeId);
        }
    }
}
