using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Exercises;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.Exercises;

/// <summary>Coach-facing CRUD over the exercise library (tenant-scoped).</summary>
[Authorize(CoachAppPermissions.Coach.Exercises.Default)]
public class ExerciseAppService : CoachAppAppService, IExerciseAppService
{
    private readonly IRepository<Exercise, Guid> _exerciseRepository;

    public ExerciseAppService(IRepository<Exercise, Guid> exerciseRepository)
    {
        _exerciseRepository = exerciseRepository;
    }

    public virtual async Task<ExerciseDto> GetAsync(Guid id)
    {
        var exercise = await _exerciseRepository.GetAsync(id);
        return ObjectMapper.Map<Exercise, ExerciseDto>(exercise);
    }

    public virtual async Task<PagedResultDto<ExerciseDto>> GetListAsync(GetExerciseListInput input)
    {
        var query = await _exerciseRepository.GetQueryableAsync();

        query = query
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                x => x.Name.Contains(input.Filter!)
                     || (x.Description != null && x.Description.Contains(input.Filter!)))
            .WhereIf(input.TargetMuscle.HasValue, x => x.TargetMuscle == input.TargetMuscle!.Value)
            .WhereIf(input.Equipment.HasValue, x => x.Equipment == input.Equipment!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(Exercise.Name)} asc"
            : input.Sorting;

        var exercises = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<ExerciseDto>(
            totalCount,
            ObjectMapper.Map<List<Exercise>, List<ExerciseDto>>(exercises));
    }

    [Authorize(CoachAppPermissions.Coach.Exercises.Create)]
    public virtual async Task<ExerciseDto> CreateAsync(CreateUpdateExerciseDto input)
    {
        var exercise = Exercise.Create(
            GuidGenerator.Create(),
            input.Name,
            input.TargetMuscle,
            input.Equipment,
            input.Description,
            input.Instructions,
            input.VideoUrl,
            input.ImageUrl,
            CurrentTenant.Id);

        exercise.IsActive = input.IsActive;

        await _exerciseRepository.InsertAsync(exercise, autoSave: true);

        return ObjectMapper.Map<Exercise, ExerciseDto>(exercise);
    }

    [Authorize(CoachAppPermissions.Coach.Exercises.Update)]
    public virtual async Task<ExerciseDto> UpdateAsync(Guid id, CreateUpdateExerciseDto input)
    {
        var exercise = await _exerciseRepository.GetAsync(id);

        exercise.Name = input.Name;
        exercise.Description = input.Description;
        exercise.Instructions = input.Instructions;
        exercise.TargetMuscle = input.TargetMuscle;
        exercise.Equipment = input.Equipment;
        exercise.VideoUrl = input.VideoUrl;
        exercise.ImageUrl = input.ImageUrl;
        exercise.IsActive = input.IsActive;

        await _exerciseRepository.UpdateAsync(exercise, autoSave: true);

        return ObjectMapper.Map<Exercise, ExerciseDto>(exercise);
    }

    [Authorize(CoachAppPermissions.Coach.Exercises.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _exerciseRepository.DeleteAsync(id);
    }
}
