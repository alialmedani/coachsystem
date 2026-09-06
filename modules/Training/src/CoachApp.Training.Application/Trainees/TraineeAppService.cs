using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoachApp.Training.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;

namespace CoachApp.Training.Trainees;

[Authorize(TrainingPermissions.Trainees.Default)]
public class TraineeAppService : TrainingAppService, ITraineeAppService
{
    private readonly ITraineeRepository _traineeRepository;
    private readonly TraineeManager _traineeManager;

    public TraineeAppService(
        ITraineeRepository traineeRepository,
        TraineeManager traineeManager)
    {
        _traineeRepository = traineeRepository;
        _traineeManager = traineeManager;
    }

    public virtual async Task<TraineeDto> GetAsync(Guid id)
    {
        var trainee = await _traineeRepository.GetAsync(id);
        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    public virtual async Task<PagedResultDto<TraineeDto>> GetListAsync(GetTraineeListInput input)
    {
        var totalCount = await _traineeRepository.GetCountAsync(
            input.Filter,
            input.Gender,
            input.IsActive);

        var trainees = await _traineeRepository.GetListAsync(
            input.Filter,
            input.Gender,
            input.IsActive,
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount);

        return new PagedResultDto<TraineeDto>(
            totalCount,
            ObjectMapper.Map<List<Trainee>, List<TraineeDto>>(trainees));
    }

    [Authorize(TrainingPermissions.Trainees.Create)]
    public virtual async Task<TraineeDto> CreateAsync(CreateUpdateTraineeDto input)
    {
        var trainee = await _traineeManager.CreateAsync(
            input.Code,
            input.FirstName,
            input.LastName,
            input.Gender,
            input.BirthDate,
            input.Email,
            input.PhoneNumber,
            input.Address);

        if (!input.IsActive)
        {
            trainee.Deactivate();
        }

        await _traineeRepository.InsertAsync(trainee, autoSave: true);

        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    [Authorize(TrainingPermissions.Trainees.Update)]
    public virtual async Task<TraineeDto> UpdateAsync(Guid id, CreateUpdateTraineeDto input)
    {
        var trainee = await _traineeRepository.GetAsync(id);

        await _traineeManager.ChangeCodeAsync(trainee, input.Code);

        trainee.SetName(input.FirstName, input.LastName);
        trainee.SetGender(input.Gender);
        trainee.SetBirthDate(input.BirthDate);
        trainee.SetEmail(input.Email);
        trainee.SetPhoneNumber(input.PhoneNumber);
        trainee.SetAddress(input.Address);

        if (input.IsActive)
        {
            trainee.Activate();
        }
        else
        {
            trainee.Deactivate();
        }

        await _traineeRepository.UpdateAsync(trainee, autoSave: true);

        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    [Authorize(TrainingPermissions.Trainees.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await _traineeRepository.DeleteAsync(id);
    }
}
