using System;
using System.Linq;
using System.Threading.Tasks;
using CoachApp.Apis;
using CoachApp.Entites.Foods;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace CoachApp.Apis.Foods;

/// <summary>Integration tests for the coach-facing <see cref="IFoodAppService"/>.</summary>
public abstract class FoodAppServiceTests<TStartupModule> : CoachAppApiTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IFoodAppService _foodAppService;

    protected FoodAppServiceTests()
    {
        _foodAppService = GetRequiredService<IFoodAppService>();
    }

    [Fact]
    public async Task Should_Create_Food_When_Input_Is_Valid()
    {
        var result = await _foodAppService.CreateAsync(new CreateUpdateFoodDto
        {
            Name = "Oats",
            ServingSize = 40m,
            ServingUnit = "g",
            Calories = 150m,
            ProteinG = 5m,
            CarbsG = 27m,
            FatG = 3m,
            IsActive = true
        });

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Oats");
        result.ServingSize.ShouldBe(40m);
        result.Calories.ShouldBe(150m);
        result.ProteinG.ShouldBe(5m);
        result.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_Get_Food_By_Id()
    {
        var created = await CreateFoodAsync("Banana", calories: 89m);

        var fetched = await _foodAppService.GetAsync(created.Id);

        fetched.Id.ShouldBe(created.Id);
        fetched.Name.ShouldBe("Banana");
        fetched.Calories.ShouldBe(89m);
    }

    [Fact]
    public async Task Should_Filter_List_By_Name()
    {
        await CreateFoodAsync("Brown Rice");
        await CreateFoodAsync("White Rice");
        await CreateFoodAsync("Almonds");

        var result = await _foodAppService.GetListAsync(new GetFoodListInput { Filter = "Rice" });

        result.TotalCount.ShouldBe(2);
        result.Items.ShouldAllBe(x => x.Name.Contains("Rice"));
    }

    [Fact]
    public async Task Should_Update_Food()
    {
        var created = await CreateFoodAsync("Milk", calories: 60m);

        var updated = await _foodAppService.UpdateAsync(created.Id, new CreateUpdateFoodDto
        {
            Name = "Skim Milk",
            ServingSize = 250m,
            ServingUnit = "ml",
            Calories = 35m,
            ProteinG = 8m,
            CarbsG = 12m,
            FatG = 0m,
            IsActive = true
        });

        updated.Name.ShouldBe("Skim Milk");
        updated.ServingUnit.ShouldBe("ml");
        updated.Calories.ShouldBe(35m);
    }

    [Fact]
    public async Task Should_Delete_Food()
    {
        var created = await CreateFoodAsync("Temp Food");

        await _foodAppService.DeleteAsync(created.Id);

        await Should.ThrowAsync<EntityNotFoundException>(() => _foodAppService.GetAsync(created.Id));
    }

    [Fact]
    public async Task Should_Throw_Validation_When_Name_Is_Missing()
    {
        await Should.ThrowAsync<AbpValidationException>(() =>
            _foodAppService.CreateAsync(new CreateUpdateFoodDto { Name = "", ServingUnit = "g" }));
    }

    [Fact]
    public async Task Should_Throw_Validation_When_ServingUnit_Is_Missing()
    {
        await Should.ThrowAsync<AbpValidationException>(() =>
            _foodAppService.CreateAsync(new CreateUpdateFoodDto { Name = "Egg", ServingUnit = "" }));
    }
}
