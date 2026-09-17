using System;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Trainees;
using CoachApp.Enums;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.Trainees;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyProfileAppService"/>. Permission-level
/// authorization (403) is not exercised (AlwaysAllowAuthorization); these assert the real enforcement:
/// the profile is resolved from the current user, and a user with no trainee profile is rejected.
/// </summary>
public abstract class MyProfileAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyProfileAppService _myProfile;

    protected MyProfileAppServiceTests()
    {
        _myProfile = GetRequiredService<IMyProfileAppService>();
    }

    [Fact]
    public async Task Should_Return_The_Current_Trainees_Own_Profile()
    {
        var trainee = await CreateTraineeAsync();

        TraineeDto profile;
        using (ChangeToTrainee(trainee))
        {
            profile = await _myProfile.GetAsync();
        }

        profile.Id.ShouldBe(trainee.Id);
        profile.UserId.ShouldBe(trainee.UserId);
    }

    [Fact]
    public async Task Should_Throw_When_Current_User_Has_No_Trainee_Profile()
    {
        // No ChangeToTrainee: the default principal is the host admin, which has no Trainee record.
        await Should.ThrowAsync<EntityNotFoundException>(() => _myProfile.GetAsync());
    }

    [Fact]
    public async Task Update_Should_Edit_Contact_Fields_Only_And_Keep_Coaching_Fields()
    {
        // PD6: the trainee may edit phone/email/birthdate; user name is immutable and coaching
        // fields (goal, targets) stay coach-owned.
        var trainee = await CreateTraineeAsync(goal: TrainingGoal.Strength);

        TraineeDto updated;
        using (ChangeToTrainee(trainee))
        {
            updated = await _myProfile.UpdateAsync(new UpdateMyProfileDto
            {
                PhoneNumber = "0700-1234567",
                Email = "trainee.self@example.com",
                BirthDate = new DateTime(1995, 6, 15)
            });
        }

        updated.PhoneNumber.ShouldBe("0700-1234567");
        updated.Email.ShouldBe("trainee.self@example.com");
        updated.BirthDate.ShouldBe(new DateTime(1995, 6, 15));

        // Immutable / coach-owned fields are untouched.
        updated.UserName.ShouldBe(trainee.UserName);
        updated.Goal.ShouldBe(TrainingGoal.Strength);
    }
}
