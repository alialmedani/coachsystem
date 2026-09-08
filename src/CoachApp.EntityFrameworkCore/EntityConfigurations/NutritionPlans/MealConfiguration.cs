using CoachApp.Entites.NutritionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionPlans;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "Meals", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(NutritionPlanConsts.MaxMealNameLength);

        b.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.MealId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.NutritionPlanId);
    }
}
