using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Trainees;
using CoachApp.Enums;
using Shouldly;
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
}
