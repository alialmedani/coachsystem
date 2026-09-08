using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.TraineeNotes;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace CoachApp.Apis.TraineeNotes;

/// <summary>Integration tests for the coach-facing <see cref="ITraineeNoteAppService"/>.</summary>
public abstract class TraineeNoteAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ITraineeNoteAppService _noteAppService;

    protected TraineeNoteAppServiceTests()
    {
        _noteAppService = GetRequiredService<ITraineeNoteAppService>();
    }

    [Fact]
    public async Task Should_Create_Note_For_Trainee()
    {
        var trainee = await CreateTraineeAsync();

        var result = await _noteAppService.CreateAsync(new CreateUpdateTraineeNoteDto
        {
            TraineeId = trainee.Id,
            Date = new DateTime(2026, 5, 1),
            Text = "Keep hydrating"
        });

        result.Id.ShouldNotBe(Guid.Empty);
        result.TraineeId.ShouldBe(trainee.Id);
        result.Text.ShouldBe("Keep hydrating");
    }

    [Fact]
    public async Task Should_Throw_When_Trainee_Does_Not_Exist()
    {
        await Should.ThrowAsync<BusinessException>(() =>
            _noteAppService.CreateAsync(new CreateUpdateTraineeNoteDto
            {
                TraineeId = Guid.NewGuid(),
                Date = DateTime.Now,
                Text = "Orphan note"
            }));
    }

    [Fact]
    public async Task Should_List_Notes_By_Trainee()
    {
        var trainee = await CreateTraineeAsync();
        await _noteAppService.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = new DateTime(2026, 1, 1), Text = "Note 1" });
        await _noteAppService.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = new DateTime(2026, 2, 1), Text = "Note 2" });

        var result = await _noteAppService.GetListAsync(new GetTraineeNoteListInput { TraineeId = trainee.Id });

        result.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task Should_Update_Note()
    {
        var trainee = await CreateTraineeAsync();
        var created = await _noteAppService.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = DateTime.Now, Text = "Original" });

        var updated = await _noteAppService.UpdateAsync(created.Id, new CreateUpdateTraineeNoteDto
        {
            TraineeId = trainee.Id,
            Date = created.Date,
            Text = "Edited"
        });

        updated.Text.ShouldBe("Edited");
    }

    [Fact]
    public async Task Should_Delete_Note()
    {
        var trainee = await CreateTraineeAsync();
        var created = await _noteAppService.CreateAsync(new CreateUpdateTraineeNoteDto { TraineeId = trainee.Id, Date = DateTime.Now, Text = "Temp" });

        await _noteAppService.DeleteAsync(created.Id);

        await Should.ThrowAsync<EntityNotFoundException>(() => _noteAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Throw_Validation_When_Text_Is_Missing()
    {
        var trainee = await CreateTraineeAsync();

        await Should.ThrowAsync<AbpValidationException>(() =>
            _noteAppService.CreateAsync(new CreateUpdateTraineeNoteDto
            {
                TraineeId = trainee.Id,
                Date = DateTime.Now,
                Text = ""
            }));
    }
}
