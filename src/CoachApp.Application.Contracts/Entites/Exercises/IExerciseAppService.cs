using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace CoachApp.Entites.Exercises;

public interface IExerciseAppService : IApplicationService
{
    Task<ExerciseDto> GetAsync(Guid id);

    Task<PagedResultDto<ExerciseDto>> GetListAsync(GetExerciseListInput input);

    Task<ExerciseDto> CreateAsync(CreateUpdateExerciseDto input);

    Task<ExerciseDto> UpdateAsync(Guid id, CreateUpdateExerciseDto input);

    Task DeleteAsync(Guid id);
}
