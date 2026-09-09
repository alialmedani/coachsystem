using System;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.TraineeNotes;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Xunit;

namespace CoachApp.Apis.TraineeNotes;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyNoteAppService"/> (read the coach's notes
/// about me). Notes are authored through the coach service, then read impersonating the trainee via
/// ChangeToTrainee. Permission-level authorization (403) is not exercised (AlwaysAllowAuthorization);
/// these assert the real enforcement: trainee resolved from the current user + per-note ownership.
/// Cross-tenant isolation isn't directly testable here (single host tenant) — same-tenant cross-trainee
/// isolation is.
/// </summary>
public abstract class MyNoteAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyNoteAppService _myNote;
    private readonly ITraineeNoteAppService _coachNote;

    protected MyNoteAppServiceTests()
    {
        _myNote = GetRequiredService<IMyNoteAppService>();
        _coachNote = GetRequiredService<ITraineeNoteAppService>();
    }

    [Fact]
    public async Task GetList_Should_Return_The_Trainees_Own_Notes()
    {
        var trainee = await CreateTraineeAsync();

        await _coachNote.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = new DateTime(2026, 1, 1), Text = "Note 1" });
        await _coachNote.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = new DateTime(2026, 2, 1), Text = "Note 2" });

        using (ChangeToTrainee(trainee))
        {
            var list = await _myNote.GetListAsync(new GetMyNoteListInput());
            list.TotalCount.ShouldBe(2);
        }
    }

    [Fact]
    public async Task Get_Should_Return_An_Own_Note()
    {
        var trainee = await CreateTraineeAsync();
        var note = await _coachNote.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = new DateTime(2026, 1, 1), Text = "Keep hydrating" });

        using (ChangeToTrainee(trainee))
        {
            var mine = await _myNote.GetAsync(note.Id);
            mine.Text.ShouldBe("Keep hydrating");
        }
    }

    [Fact]
    public async Task Should_Not_See_Another_Trainees_Notes()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();
        var note = await _coachNote.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = new DateTime(2026, 1, 1), Text = "Private" });

        using (ChangeToTrainee(other))
        {
            var list = await _myNote.GetListAsync(new GetMyNoteListInput());
            list.TotalCount.ShouldBe(0);

            await Should.ThrowAsync<EntityNotFoundException>(() => _myNote.GetAsync(note.Id));
        }
    }
}
