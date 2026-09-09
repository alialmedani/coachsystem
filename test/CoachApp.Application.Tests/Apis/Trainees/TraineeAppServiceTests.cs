using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Trainees;
using CoachApp.Enums;
using CoachApp.Permissions;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace CoachApp.Apis.Trainees;

/// <summary>
/// Integration tests for the coach-facing <see cref="ITraineeAppService"/>, which
/// provisions an <c>IdentityUser</c> together with the coaching profile.
/// </summary>
public abstract class TraineeAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ITraineeAppService _traineeAppService;
    private readonly IIdentityUserRepository _userRepository;

    protected TraineeAppServiceTests()
    {
        _traineeAppService = GetRequiredService<ITraineeAppService>();
        _userRepository = GetRequiredService<IIdentityUserRepository>();
    }

    [Fact]
    public async Task Should_Create_Trainee_And_Provision_Identity_User()
    {
        var userName = "trainee_" + Guid.NewGuid().ToString("N")[..8];

        var result = await _traineeAppService.CreateAsync(new CreateTraineeDto
        {
            UserName = userName,
            Password = ValidPassword,
            FirstName = "Jane",
            LastName = "Roe",
            Goal = TrainingGoal.LoseWeight,
            IsActive = true
        });

        result.Id.ShouldNotBe(Guid.Empty);
        result.UserId.ShouldNotBe(Guid.Empty);
        result.UserName.ShouldBe(userName);
        result.FirstName.ShouldBe("Jane");
        result.Goal.ShouldBe(TrainingGoal.LoseWeight);

        await WithUnitOfWorkAsync(async () =>
        {
            var user = await _userRepository.FindAsync(result.UserId);
            user.ShouldNotBeNull();
            user!.UserName.ShouldBe(userName);
        });
    }

    [Fact]
    public async Task Should_Get_Trainee_By_Id()
    {
        var created = await CreateTraineeAsync(firstName: "Mike");

        var fetched = await _traineeAppService.GetAsync(created.Id);

        fetched.Id.ShouldBe(created.Id);
        fetched.FirstName.ShouldBe("Mike");
    }

    [Fact]
    public async Task Should_Filter_List_By_Name()
    {
        await CreateTraineeAsync(firstName: "Alexander");
        await CreateTraineeAsync(firstName: "Beatrice");

        var result = await _traineeAppService.GetListAsync(new GetTraineeListInput { Filter = "Alexander" });

        result.TotalCount.ShouldBe(1);
        result.Items.Single().FirstName.ShouldBe("Alexander");
    }

    [Fact]
    public async Task Should_Update_Trainee_Profile()
    {
        var created = await CreateTraineeAsync(firstName: "Old", lastName: "Name");

        var updated = await _traineeAppService.UpdateAsync(created.Id, new UpdateTraineeDto
        {
            FirstName = "New",
            LastName = "Name",
            Goal = TrainingGoal.Strength,
            HeightCm = 175m,
            IsActive = true
        });

        updated.FirstName.ShouldBe("New");
        updated.Goal.ShouldBe(TrainingGoal.Strength);
        updated.HeightCm.ShouldBe(175m);
    }

    [Fact]
    public async Task Should_Delete_Trainee_And_Its_Identity_User()
    {
        var created = await CreateTraineeAsync();

        await _traineeAppService.DeleteAsync(created.Id);

        await Should.ThrowAsync<EntityNotFoundException>(() => _traineeAppService.GetAsync(created.Id));

        await WithUnitOfWorkAsync(async () =>
        {
            var user = await _userRepository.FindAsync(created.UserId);
            user.ShouldBeNull();
        });
    }

    [Fact]
    public async Task Should_Throw_Validation_When_Password_Too_Short()
    {
        await Should.ThrowAsync<AbpValidationException>(() =>
            _traineeAppService.CreateAsync(new CreateTraineeDto
            {
                UserName = "trainee_" + Guid.NewGuid().ToString("N")[..8],
                Password = "123",
                FirstName = "Jane",
                LastName = "Roe"
            }));
    }

    [Fact]
    public async Task Should_Throw_Validation_When_UserName_Is_Missing()
    {
        await Should.ThrowAsync<AbpValidationException>(() =>
            _traineeAppService.CreateAsync(new CreateTraineeDto
            {
                UserName = "",
                Password = ValidPassword,
                FirstName = "Jane",
                LastName = "Roe"
            }));
    }

    [Fact]
    public async Task ResetPassword_Should_Change_The_Trainee_Password()
    {
        var trainee = await CreateTraineeAsync();
        const string newPassword = "NewPass123!";

        await _traineeAppService.ResetPasswordAsync(trainee.Id, new ResetTraineePasswordDto { NewPassword = newPassword });

        var userManager = GetRequiredService<IdentityUserManager>();
        await WithUnitOfWorkAsync(async () =>
        {
            var user = await userManager.FindByIdAsync(trainee.UserId.ToString());
            user.ShouldNotBeNull();
            (await userManager.CheckPasswordAsync(user!, newPassword)).ShouldBeTrue();    // new password works
            (await userManager.CheckPasswordAsync(user!, ValidPassword)).ShouldBeFalse(); // old password no longer works
        });
    }

    [Fact]
    public async Task ResetPassword_Should_Fail_Identity_Policy_For_Weak_Password()
    {
        var trainee = await CreateTraineeAsync();

        // Passes DTO length validation (>= 6 chars) but violates the ABP Identity password policy
        // (no uppercase / digit / symbol) — surfaces through the existing CheckIdentityErrors path.
        await Should.ThrowAsync<UserFriendlyException>(() =>
            _traineeAppService.ResetPasswordAsync(trainee.Id, new ResetTraineePasswordDto { NewPassword = "abcdef" }));
    }

    [Fact]
    public async Task ResetPassword_Should_Throw_Validation_For_Too_Short_Password()
    {
        var trainee = await CreateTraineeAsync();

        await Should.ThrowAsync<AbpValidationException>(() =>
            _traineeAppService.ResetPasswordAsync(trainee.Id, new ResetTraineePasswordDto { NewPassword = "123" }));
    }

    [Fact]
    public async Task ResetPassword_Should_Throw_For_Nonexistent_Trainee()
    {
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _traineeAppService.ResetPasswordAsync(Guid.NewGuid(), new ResetTraineePasswordDto { NewPassword = "NewPass123!" }));
    }

    [Fact]
    public async Task ResetPassword_Cannot_Target_An_Arbitrary_Identity_User()
    {
        var trainee = await CreateTraineeAsync();

        // {id} is the trainee id, not the Identity user id. Passing the Identity user's id (or any
        // non-trainee id) resolves no trainee in the tenant → not found. This is also the mechanism
        // that isolates trainees/users belonging to other tenants.
        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _traineeAppService.ResetPasswordAsync(trainee.UserId, new ResetTraineePasswordDto { NewPassword = "NewPass123!" }));
    }

    [Fact]
    public async Task ResetPassword_Permission_Is_Registered()
    {
        // The harness uses AlwaysAllowAuthorization, so a real 403 isn't exercised; instead verify
        // the dedicated permission is defined (the endpoint carries [Authorize(...ResetPassword)]).
        var permissionManager = GetRequiredService<IPermissionDefinitionManager>();
        var permission = await permissionManager.GetOrNullAsync(CoachAppPermissions.Coach.Trainees.ResetPassword);
        permission.ShouldNotBeNull();
    }
}
