using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.NutritionLogs;
using CoachApp.Entites.NutritionPlans;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace CoachApp.Apis.NutritionLogs;

/// <summary>
/// Integration tests for the trainee-facing <see cref="IMyNutritionLogAppService"/> (self-logging).
/// All calls run impersonating the trainee via ChangeToTrainee. NOTE: permission-level authorization
/// (403) is not exercised — the test host uses AlwaysAllowAuthorization — so these assert the real
/// in-code enforcement: trainee resolved from the current user + per-log ownership. Cross-tenant
/// isolation isn't directly testable here (single host tenant); same-tenant cross-trainee isolation
/// is the tested analog.
/// </summary>
public abstract class MyNutritionLogAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IMyNutritionLogAppService _myLog;
    private readonly INutritionPlanAppService _planAppService;

    protected MyNutritionLogAppServiceTests()
    {
        _myLog = GetRequiredService<IMyNutritionLogAppService>();
        _planAppService = GetRequiredService<INutritionPlanAppService>();
    }

    [Fact]
    public async Task Create_Should_Log_For_The_Current_Trainee_With_Computed_Totals()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Oats"); // 100 kcal / serving by default

        NutritionLogDto log;
        using (ChangeToTrainee(trainee))
        {
            log = await _myLog.CreateAsync(new CreateNutritionLogDto
            {
                Date = new DateTime(2026, 4, 1),
                Notes = "breakfast",
                Entries = new List<CreateNutritionLogEntryDto>
                {
                    new() { FoodId = food.Id, Order = 1, Quantity = 2m }
                }
            });
        }

        log.TraineeId.ShouldBe(trainee.Id); // server-set from the current user, not the client
        log.Entries.Count.ShouldBe(1);
        log.TotalCalories.ShouldBe(200m);   // 100 * 2
    }

    [Fact]
    public async Task Create_Should_Throw_For_Unknown_Food()
    {
        var trainee = await CreateTraineeAsync();

        using (ChangeToTrainee(trainee))
        {
            var ex = await Should.ThrowAsync<BusinessException>(() =>
                _myLog.CreateAsync(new CreateNutritionLogDto
                {
                    Date = new DateTime(2026, 4, 1),
                    Entries = new List<CreateNutritionLogEntryDto>
                    {
                        new() { FoodId = Guid.NewGuid(), Order = 1, Quantity = 1m }
                    }
                }));
            ex.Code.ShouldBe(CoachAppDomainErrorCodes.FoodsNotFound);
        }
    }

    [Fact]
    public async Task List_Should_Return_Only_Own_Logs_With_Date_Filter()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        using (ChangeToTrainee(trainee))
        {
            await LogOnAsync(food.Id, new DateTime(2026, 1, 1));
            await LogOnAsync(food.Id, new DateTime(2026, 2, 1));
            await LogOnAsync(food.Id, new DateTime(2026, 3, 1));
        }

        using (ChangeToTrainee(other))
        {
            await LogOnAsync(food.Id, new DateTime(2026, 1, 15));
        }

        using (ChangeToTrainee(trainee))
        {
            var all = await _myLog.GetListAsync(new GetMyNutritionLogListInput());
            all.TotalCount.ShouldBe(3); // isolation: not 4

            var filtered = await _myLog.GetListAsync(new GetMyNutritionLogListInput { FromDate = new DateTime(2026, 2, 1) });
            filtered.TotalCount.ShouldBe(2);
        }

        using (ChangeToTrainee(other))
        {
            var mine = await _myLog.GetListAsync(new GetMyNutritionLogListInput());
            mine.TotalCount.ShouldBe(1);
        }
    }

    [Fact]
    public async Task Get_Should_Throw_For_A_Log_Owned_By_Another_Trainee()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        Guid logId;
        using (ChangeToTrainee(trainee))
        {
            logId = await LogOnAsync(food.Id, new DateTime(2026, 4, 1));
        }

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _myLog.GetAsync(logId));
        }
    }

    [Fact]
    public async Task Delete_Should_Remove_Own_And_Reject_Foreign()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        Guid logId;
        using (ChangeToTrainee(trainee))
        {
            logId = await LogOnAsync(food.Id, new DateTime(2026, 4, 1));
        }

        // Another trainee cannot delete it.
        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _myLog.DeleteAsync(logId));
        }

        // The owner can, and it's gone afterward.
        using (ChangeToTrainee(trainee))
        {
            await _myLog.DeleteAsync(logId);
            await Should.ThrowAsync<EntityNotFoundException>(() => _myLog.GetAsync(logId));
        }
    }

    [Fact]
    public async Task Update_Should_Replace_Entries_For_Own_Log()
    {
        var trainee = await CreateTraineeAsync();
        var food1 = await CreateFoodAsync("Oats");
        var food2 = await CreateFoodAsync("Whey", calories: 120m);

        NutritionLogDto updated;
        using (ChangeToTrainee(trainee))
        {
            var log = await _myLog.CreateAsync(new CreateNutritionLogDto
            {
                Date = new DateTime(2026, 4, 1),
                Notes = "before",
                Entries = new List<CreateNutritionLogEntryDto> { new() { FoodId = food1.Id, Order = 1, Quantity = 1m } }
            });

            updated = await _myLog.UpdateAsync(log.Id, new UpdateNutritionLogDto
            {
                Date = new DateTime(2026, 4, 2),
                Notes = "after",
                Entries = new List<CreateNutritionLogEntryDto> { new() { FoodId = food2.Id, Order = 1, Quantity = 3m } }
            });
        }

        updated.Notes.ShouldBe("after");
        updated.Date.ShouldBe(new DateTime(2026, 4, 2));
        updated.Entries.Count.ShouldBe(1);
        updated.Entries.Single().FoodId.ShouldBe(food2.Id);
        updated.Entries.Single().Quantity.ShouldBe(3m);
        updated.TotalCalories.ShouldBe(360m); // 120 * 3
    }

    [Fact]
    public async Task Update_Should_Throw_For_A_Log_Owned_By_Another_Trainee()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        Guid logId;
        using (ChangeToTrainee(trainee))
        {
            logId = await LogOnAsync(food.Id, new DateTime(2026, 4, 1));
        }

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.UpdateAsync(logId, new UpdateNutritionLogDto { Date = new DateTime(2026, 4, 1) }));
        }
    }

    [Fact]
    public async Task Update_Should_Throw_For_Unknown_Food()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        using (ChangeToTrainee(trainee))
        {
            var log = await _myLog.CreateAsync(new CreateNutritionLogDto
            {
                Date = new DateTime(2026, 4, 1),
                Entries = new List<CreateNutritionLogEntryDto> { new() { FoodId = food.Id, Order = 1, Quantity = 1m } }
            });

            var ex = await Should.ThrowAsync<BusinessException>(() =>
                _myLog.UpdateAsync(log.Id, new UpdateNutritionLogDto
                {
                    Date = new DateTime(2026, 4, 1),
                    Entries = new List<CreateNutritionLogEntryDto> { new() { FoodId = Guid.NewGuid(), Order = 1, Quantity = 1m } }
                }));
            ex.Code.ShouldBe(CoachAppDomainErrorCodes.FoodsNotFound);
        }
    }

    [Fact]
    public async Task CreateFromPlan_Should_Snapshot_Own_Plan_Into_Log()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync("Chicken");

        var plan = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Cut",
            IsActive = true,
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Lunch", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 2m } } }
            }
        });

        NutritionLogDto log;
        using (ChangeToTrainee(trainee))
        {
            log = await _myLog.CreateFromPlanAsync(new CreateNutritionLogFromPlanDto
            {
                NutritionPlanId = plan.Id,
                Date = new DateTime(2026, 4, 1),
                Notes = "as planned"
            });
        }

        log.NutritionPlanId.ShouldBe(plan.Id);
        log.Entries.Count.ShouldBe(1);
        log.Entries.Single().FoodId.ShouldBe(food.Id);
        log.Entries.Single().Quantity.ShouldBe(2m);
        log.TotalCalories.ShouldBe(200m); // 100 * 2
    }

    [Fact]
    public async Task CreateFromPlan_Should_Throw_When_Plan_Belongs_To_Another_Trainee()
    {
        var trainee = await CreateTraineeAsync();
        var other = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        var plan = await _planAppService.CreateAsync(new CreateUpdateNutritionPlanDto
        {
            TraineeId = trainee.Id,
            Name = "Plan",
            Meals = new List<CreateUpdateMealDto>
            {
                new() { Name = "Meal", Order = 1, Items = new List<CreateUpdateMealItemDto> { new() { FoodId = food.Id, Order = 1, Quantity = 1m } } }
            }
        });

        using (ChangeToTrainee(other))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.CreateFromPlanAsync(new CreateNutritionLogFromPlanDto
                {
                    NutritionPlanId = plan.Id,
                    Date = new DateTime(2026, 4, 1)
                }));
        }
    }

    [Fact]
    public async Task CreateFromPlan_Should_Throw_When_Plan_Is_Unknown()
    {
        var trainee = await CreateTraineeAsync();

        using (ChangeToTrainee(trainee))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _myLog.CreateFromPlanAsync(new CreateNutritionLogFromPlanDto
                {
                    NutritionPlanId = Guid.NewGuid(),
                    Date = new DateTime(2026, 4, 1)
                }));
        }
    }

    [Fact]
    public async Task Create_Should_Reject_More_Than_Max_Entries()
    {
        var trainee = await CreateTraineeAsync();
        var food = await CreateFoodAsync();

        var entries = Enumerable.Range(1, NutritionLogConsts.MaxEntries + 1)
            .Select(i => new CreateNutritionLogEntryDto { FoodId = food.Id, Order = i, Quantity = 1m })
            .ToList();

        using (ChangeToTrainee(trainee))
        {
            await Should.ThrowAsync<AbpValidationException>(() =>
                _myLog.CreateAsync(new CreateNutritionLogDto
                {
                    Date = new DateTime(2026, 4, 1),
                    Entries = entries
                }));
        }
    }

    private async Task<Guid> LogOnAsync(Guid foodId, DateTime date)
    {
        var log = await _myLog.CreateAsync(new CreateNutritionLogDto
        {
            Date = date,
            Entries = new List<CreateNutritionLogEntryDto> { new() { FoodId = foodId, Order = 1, Quantity = 1m } }
        });
        return log.Id;
    }
}
