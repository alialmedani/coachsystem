using CoachApp.Entites.NutritionPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionPlans;

public class NutritionPlanConfiguration : IEntityTypeConfiguration<NutritionPlan>
{
    public void Configure(EntityTypeBuilder<NutritionPlan> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "NutritionPlans", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(NutritionPlanConsts.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(NutritionPlanConsts.MaxDescriptionLength);

        b.HasMany(x => x.Meals)
            .WithOne()
            .HasForeignKey(x => x.NutritionPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.TenantId, x.TraineeId });
    }
}
