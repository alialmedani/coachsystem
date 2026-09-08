using CoachApp.Entites.NutritionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionPlans;

public class MealItemConfiguration : IEntityTypeConfiguration<MealItem>
{
    public void Configure(EntityTypeBuilder<MealItem> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "MealItems", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Quantity).HasColumnType("decimal(9,2)");

        b.HasIndex(x => x.MealId);
        b.HasIndex(x => x.FoodId);
    }
}
