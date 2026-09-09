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

/// <summary>Integration tests for the coach-facing <see cref="IProgressEntryAppService"/>.</summary>
public abstract class ProgressEntryAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IProgressEntryAppService _progressAppService;

    protected ProgressEntryAppServiceTests()
    {
        _progressAppService = GetRequiredService<IProgressEntryAppService>();
    }

    [Fact]
    public async Task Should_Create_Entry_For_Trainee()
    {
        var trainee = await CreateTraineeAsync();

        var result = await _progressAppService.CreateAsync(new CreateUpdateProgressEntryDto
        {
            TraineeId = trainee.Id,
            Date = new DateTime(2026, 4, 1),
            WeightKg = 80m,
            BodyFatPercent = 20m,
            Notes = "Baseline"
        });

        result.Id.ShouldNotBe(Guid.Empty);
        result.TraineeId.ShouldBe(trainee.Id);
        result.WeightKg.ShouldBe(80m);
        result.Notes.ShouldBe("Baseline");
    }

    [Fact]
    public async Task Should_Throw_When_Trainee_Does_Not_Exist()
    {
        var ex = await Should.ThrowAsync<BusinessException>(() =>
            _progressAppService.CreateAsync(new CreateUpdateProgressEntryDto
            {
                TraineeId = Guid.NewGuid(),
                Date = DateTime.Now,
                WeightKg = 70m
            }));
        ex.Code.ShouldBe(CoachAppDomainErrorCodes.TraineeNotFound);
    }

    [Fact]
    public async Task Should_List_By_Trainee_With_Date_Filter()
    {
        var trainee = await CreateTraineeAsync();

        await _progressAppService.CreateAsync(new CreateUpdateProgressEntryDto { TraineeId = trainee.Id, Date = new DateTime(2026, 1, 1), WeightKg = 82m });
        await _progressAppService.CreateAsync(new CreateUpdateProgressEntryDto { TraineeId = trainee.Id, Date = new DateTime(2026, 2, 1), WeightKg = 81m });
        await _progressAppService.CreateAsync(new CreateUpdateProgressEntryDto { TraineeId = trainee.Id, Date = new DateTime(2026, 3, 1), WeightKg = 80m });

        var all = await _progressAppService.GetListAsync(new GetProgressEntryListInput { TraineeId = trainee.Id });
        all.TotalCount.ShouldBe(3);

        var filtered = await _progressAppService.GetListAsync(new GetProgressEntryListInput
        {
            TraineeId = trainee.Id,
            FromDate = new DateTime(2026, 2, 1)
        });
        filtered.TotalCount.ShouldBe(2);
    }

    [Fact]
    public async Task Should_Update_Entry()
    {
        var trainee = await CreateTraineeAsync();
        var created = await _progressAppService.CreateAsync(new CreateUpdateProgressEntryDto { TraineeId = trainee.Id, Date = DateTime.Now, WeightKg = 90m });

        var updated = await _progressAppService.UpdateAsync(created.Id, new CreateUpdateProgressEntryDto
        {
            TraineeId = trainee.Id,
            Date = created.Date,
            WeightKg = 88m,
            Notes = "Down 2kg"
        });

        updated.WeightKg.ShouldBe(88m);
        updated.Notes.ShouldBe("Down 2kg");
    }

    [Fact]
    public async Task Should_Delete_Entry()
    {
        var trainee = await CreateTraineeAsync();
        var created = await _progressAppService.CreateAsync(new CreateUpdateProgressEntryDto { TraineeId = trainee.Id, Date = DateTime.Now });

        await _progressAppService.DeleteAsync(created.Id);

        await Should.ThrowAsync<EntityNotFoundException>(() => _progressAppService.GetAsync(created.Id));
    }
}
