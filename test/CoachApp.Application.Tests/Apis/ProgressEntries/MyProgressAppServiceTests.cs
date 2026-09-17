using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.ProgressEntries;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.ProgressEntries;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyProgressAppService"/> (self-service progress).
/// All calls impersonate the trainee via ChangeToTrainee. Permission-level authorization (403) is not
/// exercised (AlwaysAllowAuthorization); these assert the real enforcement: trainee resolved from the
/// current user + per-entry ownership. Cross-tenant isolation isn't directly testable (single host
/// tenant); same-tenant cross-trainee isolation is the tested analog.
/// </summary>
public abstract class MyProgressAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyProgressAppService _myProgress;
    private readonly IProgressEntryAppService _coachProgress;

    protected MyProgressAppServiceTests()
    {
        _myProgress = GetRequiredService<IMyProgressAppService>();
        _coachProgress = GetRequiredService<IProgressEntryAppService>();
    }

    [Fact]
    public async Task Create_Should_Log_For_The_Current_Trainee()
    {
        var trainee = await CreateTraineeAsync();

        ProgressEntryDto entry;
        using (ChangeToTrainee(trainee))
        {
            entry = await _myProgress.CreateAsync(new CreateMyProgressEntryDto
            {
                Date = new DateTime(2026, 4, 1),
                WeightKg = 80m,
                BodyFatPercent = 18m,
                Notes = "baseline"
            });
        }

        entry.TraineeId.ShouldBe(trainee.Id); // server-set from the current user
        entry.WeightKg.ShouldBe(80m);
        entry.Notes.ShouldBe("baseline");
    }

    [Fact]
    public async Task List_Should_Return_Only_Own_Entries_With_Date_Filter()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        using (ChangeToTrainee(trainee))
        {
            await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 1, 1), WeightKg = 82m });
            await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 2, 1), WeightKg = 81m });
            await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 3, 1), WeightKg = 80m });
        }

        using (ChangeToTrainee(other))
        {
            await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 1, 20), WeightKg = 70m });
        }

        using (ChangeToTrainee(trainee))
        {
            var all = await _myProgress.GetListAsync(new GetMyProgressListInput());
            all.TotalCount.ShouldBe(3); // isolation: not 4

            var filtered = await _myProgress.GetListAsync(new GetMyProgressListInput { FromDate = new DateTime(2026, 2, 1) });
            filtered.TotalCount.ShouldBe(2);
        }

        using (ChangeToTrainee(other))
        {
            var mine = await _myProgress.GetListAsync(new GetMyProgressListInput());
            mine.TotalCount.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Get_Should_Throw_For_An_Entry_Owned_By_Another_Trainee()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        Guid entryId;
        using (ChangeToTrainee(trainee))
        {
            entryId = (await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 4, 1), WeightKg = 80m })).Id;
        }

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _myProgress.GetAsync(entryId));
        }
    }

    [Fact]
    public async Task Delete_Should_Remove_Own_And_Reject_Foreign()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        Guid entryId;
        using (ChangeToTrainee(trainee))
        {
            entryId = (await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 4, 1), WeightKg = 80m })).Id;
        }

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _myProgress.DeleteAsync(entryId));
        }

        using (ChangeToTrainee(trainee))
        {
            await _myProgress.DeleteAsync(entryId);
            await Should.ThrowAsync<EntityNotFoundException>(() => _myProgress.GetAsync(entryId));
        }
    }

    [Fact]
    public async Task Update_Should_Modify_Own_Entry()
    {
        // F3/PD4: the trainee can edit an entry they authored, and read it back.
        var trainee = await CreateTraineeAsync();

        using (ChangeToTrainee(trainee))
        {
            var created = await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 4, 1), WeightKg = 80m });

            var updated = await _myProgress.UpdateAsync(created.Id, new UpdateMyProgressEntryDto
            {
                Date = created.Date,
                WeightKg = 78.5m,
                Notes = "corrected"
            });

            updated.WeightKg.ShouldBe(78.5m);
            updated.Notes.ShouldBe("corrected");
            updated.IsCoachAuthored.ShouldBeFalse();

            (await _myProgress.GetAsync(created.Id)).WeightKg.ShouldBe(78.5m); // readable back
        }
    }

    [Fact]
    public async Task Update_Should_Reject_A_Foreign_Entry()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();

        Guid entryId;
        using (ChangeToTrainee(trainee))
        {
            entryId = (await _myProgress.CreateAsync(new CreateMyProgressEntryDto { Date = new DateTime(2026, 4, 1), WeightKg = 80m })).Id;
        }

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myProgress.UpdateAsync(entryId, new UpdateMyProgressEntryDto { Date = new DateTime(2026, 4, 1), WeightKg = 60m }));
        }
    }

    [Fact]
    public async Task Trainee_Cannot_Edit_Or_Delete_A_Coach_Authored_Entry_And_Sees_Attribution()
    {
        // F4/PD4: a coach-created entry appears on the trainee's timeline flagged as coach-authored,
        // and the trainee may neither delete nor edit it.
        var trainee = await CreateTraineeAsync();

        // Coach (default principal) records an entry for the trainee.
        var coachEntry = await _coachProgress.CreateAsync(new CreateUpdateProgressEntryDto
        {
            TraineeId = trainee.Id,
            Date = new DateTime(2026, 4, 1),
            WeightKg = 85m
        });

        using (ChangeToTrainee(trainee))
        {
            var mine = await _myProgress.GetListAsync(new GetMyProgressListInput());
            var seen = mine.Items.Single(e => e.Id == coachEntry.Id);
            seen.IsCoachAuthored.ShouldBeTrue();

            (await Should.ThrowAsync<BusinessException>(() => _myProgress.DeleteAsync(coachEntry.Id)))
                .Code.ShouldBe(CoachAppDomainErrorCodes.CannotModifyCoachAuthoredProgress);

            (await Should.ThrowAsync<BusinessException>(() =>
                _myProgress.UpdateAsync(coachEntry.Id, new UpdateMyProgressEntryDto { Date = coachEntry.Date, WeightKg = 70m })))
                .Code.ShouldBe(CoachAppDomainErrorCodes.CannotModifyCoachAuthoredProgress);
        }
    }
}
