using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.Trainees;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace CoachApp.Apis.Trainees;

[Authorize(CoachAppPermissions.Trainees.Default)]
public class TraineeAppService : CoachAppAppService, ITraineeAppService
{
    private readonly IRepository<Trainee, Guid> _traineeRepository;

    public TraineeAppService(IRepository<Trainee, Guid> traineeRepository)
    {
        _traineeRepository = traineeRepository;
    }

    public virtual async Task<TraineeDto> GetAsync(Guid id)
    {
        var trainee = await _traineeRepository.GetAsync(id);
        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    public virtual async Task<PagedResultDto<TraineeDto>> GetListAsync(GetTraineeListInput input)
    {
        var query = await _traineeRepository.GetQueryableAsync();

        query = query
            .WhereIf(
                !string.IsNullOrWhiteSpace(input.Filter),
                x => x.Code.Contains(input.Filter!)
                     || x.FirstName.Contains(input.Filter!)
                     || x.LastName.Contains(input.Filter!))
            .WhereIf(input.Gender.HasValue, x => x.Gender == input.Gender!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(Trainee.Code)} asc"
            : input.Sorting;

        var trainees = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<TraineeDto>(
            totalCount,
            ObjectMapper.Map<List<Trainee>, List<TraineeDto>>(trainees));
    }

    [Authorize(CoachAppPermissions.Trainees.Create)]
    public virtual async Task<TraineeDto> CreateAsync(CreateUpdateTraineeDto input)
    {
        await CheckCodeDuplicationAsync(input.Code);

        var trainee = Trainee.Create(
            GuidGenerator.Create(),
            input.Code,
            input.FirstName,
            input.LastName,
            input.Gender,
            input.BirthDate,
            input.Email,
            input.PhoneNumber,
            input.Address,
            CurrentTenant.Id);

        trainee.IsActive = input.IsActive;

        await _traineeRepository.InsertAsync(trainee, autoSave: true);

        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    [Authorize(CoachAppPermissions.Trainees.Update)]
    public virtual async Task<TraineeDto> UpdateAsync(Guid id, CreateUpdateTraineeDto input)
    {
        var trainee = await _traineeRepository.GetAsync(id);

        await CheckCodeDuplicationAsync(input.Code, id);

        trainee.Code = input.Code;
        trainee.FirstName = input.FirstName;
        trainee.LastName = input.LastName;
        trainee.Gender = input.Gender;
        trainee.BirthDate = input.BirthDate;
        trainee.Email = input.Email;
        trainee.PhoneNumber = input.PhoneNumber;
        trainee.Address = input.Address;
        trainee.IsActive = input.IsActive;

        await _traineeRepository.UpdateAsync(trainee, autoSave: true);

        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    [Authorize(CoachAppPermissions.Trainees.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _traineeRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Trainee code uniqueness is a cross-aggregate rule that needs repository
    /// access, so it lives here in the (fat) application service rather than on the
    /// entity.
    /// </summary>
    protected virtual async Task CheckCodeDuplicationAsync(string code, Guid? excludedId = null)
    {
        var existing = await _traineeRepository.FirstOrDefaultAsync(x => x.Code == code);
        if (existing != null && existing.Id != excludedId)
        {
            throw new BusinessException(CoachAppDomainErrorCodes.TraineeCodeAlreadyExists)
                .WithData("Code", code);
        }
    }
}
