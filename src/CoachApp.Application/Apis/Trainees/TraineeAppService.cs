using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.ProgressEntries;
using CoachApp.Entites.TraineeNotes;
using CoachApp.Entites.Trainees;
using CoachApp.Entites.WorkoutLogs;
using CoachApp.Entites.WorkoutPlans;
using CoachApp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace CoachApp.Apis.Trainees;

/// <summary>
/// Coach-facing management of trainees. Creating a trainee provisions the login
/// (an <see cref="IdentityUser"/> in the "Trainee" role, inside the coach's tenant)
/// and the coaching profile in one operation. A trainee's active state is kept in
/// sync with their login (an inactive trainee cannot sign in), and deleting a trainee
/// removes their dependent data (no orphans). Tenant isolation is automatic.
/// </summary>
[Authorize(CoachAppPermissions.Coach.Trainees.Default)]
public class TraineeAppService : CoachAppAppService, ITraineeAppService
{
    private readonly IRepository<Trainee, Guid> _traineeRepository;
    private readonly IdentityUserManager _userManager;
    private readonly IRepository<WorkoutPlan, Guid> _workoutPlanRepository;
    private readonly IRepository<NutritionPlan, Guid> _nutritionPlanRepository;
    private readonly IRepository<WorkoutLog, Guid> _workoutLogRepository;
    private readonly IRepository<NutritionLog, Guid> _nutritionLogRepository;
    private readonly IRepository<ProgressEntry, Guid> _progressRepository;
    private readonly IRepository<TraineeNote, Guid> _noteRepository;

    public TraineeAppService(
        IRepository<Trainee, Guid> traineeRepository,
        IdentityUserManager userManager,
        IRepository<WorkoutPlan, Guid> workoutPlanRepository,
        IRepository<NutritionPlan, Guid> nutritionPlanRepository,
        IRepository<WorkoutLog, Guid> workoutLogRepository,
        IRepository<NutritionLog, Guid> nutritionLogRepository,
        IRepository<ProgressEntry, Guid> progressRepository,
        IRepository<TraineeNote, Guid> noteRepository)
    {
        _traineeRepository = traineeRepository;
        _userManager = userManager;
        _workoutPlanRepository = workoutPlanRepository;
        _nutritionPlanRepository = nutritionPlanRepository;
        _workoutLogRepository = workoutLogRepository;
        _nutritionLogRepository = nutritionLogRepository;
        _progressRepository = progressRepository;
        _noteRepository = noteRepository;
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
                x => x.UserName.Contains(input.Filter!)
                     || x.FirstName.Contains(input.Filter!)
                     || x.LastName.Contains(input.Filter!))
            .WhereIf(input.Goal.HasValue, x => x.Goal == input.Goal!.Value)
            .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive!.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? $"{nameof(Trainee.FirstName)} asc"
            : input.Sorting;

        var trainees = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<TraineeDto>(
            totalCount,
            ObjectMapper.Map<List<Trainee>, List<TraineeDto>>(trainees));
    }

    [Authorize(CoachAppPermissions.Coach.Trainees.Create)]
    public virtual async Task<TraineeDto> CreateAsync(CreateTraineeDto input)
    {
        // 1) Create the login user in the coach's tenant, in the "Trainee" role.
        var email = string.IsNullOrWhiteSpace(input.Email)
            ? $"{input.UserName}@coachapp.local"
            : input.Email!;

        var user = new IdentityUser(GuidGenerator.Create(), input.UserName, email, CurrentTenant.Id);
        CheckIdentityErrors(await _userManager.CreateAsync(user, input.Password));
        CheckIdentityErrors(await _userManager.AddToRoleAsync(user, CoachAppRoles.Trainee));

        // 2) Create the coaching profile bound to that user.
        var trainee = Trainee.Create(
            GuidGenerator.Create(),
            user.Id,
            input.UserName,
            input.FirstName,
            input.LastName,
            input.Gender,
            input.BirthDate,
            input.Email,
            input.PhoneNumber,
            input.Goal,
            input.HeightCm,
            input.StartWeightKg,
            input.TargetWeightKg,
            CurrentTenant.Id);

        trainee.IsActive = input.IsActive;

        await _traineeRepository.InsertAsync(trainee, autoSave: true);

        // Keep the login in sync: a trainee created inactive cannot sign in.
        await ApplyLoginStateAsync(user, trainee.IsActive);

        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    [Authorize(CoachAppPermissions.Coach.Trainees.Update)]
    public virtual async Task<TraineeDto> UpdateAsync(Guid id, UpdateTraineeDto input)
    {
        var trainee = await _traineeRepository.GetAsync(id);

        trainee.FirstName = input.FirstName;
        trainee.LastName = input.LastName;
        trainee.Email = input.Email;
        trainee.PhoneNumber = input.PhoneNumber;
        trainee.Gender = input.Gender;
        trainee.BirthDate = input.BirthDate;
        trainee.Goal = input.Goal;
        trainee.HeightCm = input.HeightCm;
        trainee.StartWeightKg = input.StartWeightKg;
        trainee.TargetWeightKg = input.TargetWeightKg;
        trainee.IsActive = input.IsActive;

        await _traineeRepository.UpdateAsync(trainee, autoSave: true);

        // Keep the login in sync with the trainee's active state.
        var user = await _userManager.FindByIdAsync(trainee.UserId.ToString());
        if (user != null)
        {
            await ApplyLoginStateAsync(user, trainee.IsActive);
        }

        return ObjectMapper.Map<Trainee, TraineeDto>(trainee);
    }

    [Authorize(CoachAppPermissions.Coach.Trainees.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var trainee = await _traineeRepository.GetAsync(id);

        // Remove the trainee's dependent data so nothing is left orphaned. (Plan/log
        // child rows cascade at the database level.)
        await _workoutLogRepository.DeleteAsync(x => x.TraineeId == id);
        await _nutritionLogRepository.DeleteAsync(x => x.TraineeId == id);
        await _workoutPlanRepository.DeleteAsync(x => x.TraineeId == id);
        await _nutritionPlanRepository.DeleteAsync(x => x.TraineeId == id);
        await _progressRepository.DeleteAsync(x => x.TraineeId == id);
        await _noteRepository.DeleteAsync(x => x.TraineeId == id);

        await _traineeRepository.DeleteAsync(trainee, autoSave: true);

        var user = await _userManager.FindByIdAsync(trainee.UserId.ToString());
        if (user != null)
        {
            CheckIdentityErrors(await _userManager.DeleteAsync(user));
        }
    }

    /// <summary>
    /// Enables or blocks a trainee's login by clearing or setting a permanent lockout.
    /// (ABP's sign-in / password-grant flow rejects a locked-out user.)
    /// </summary>
    private async Task ApplyLoginStateAsync(IdentityUser user, bool isActive)
    {
        if (isActive)
        {
            CheckIdentityErrors(await _userManager.SetLockoutEndDateAsync(user, null));
        }
        else
        {
            CheckIdentityErrors(await _userManager.SetLockoutEnabledAsync(user, true));
            CheckIdentityErrors(await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue));
        }
    }

    private static void CheckIdentityErrors(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new UserFriendlyException(
                result.Errors.Select(e => e.Description).JoinAsString(" "));
        }
    }
}
