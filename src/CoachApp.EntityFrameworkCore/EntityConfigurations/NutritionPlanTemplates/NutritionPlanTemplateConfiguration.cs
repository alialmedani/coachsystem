using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.NutritionPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionPlanTemplates;

public class NutritionPlanTemplateConfiguration : IEntityTypeConfiguration<NutritionPlanTemplate>
{
    public void Configure(EntityTypeBuilder<NutritionPlanTemplate> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "NutritionPlanTemplates", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(NutritionPlanConsts.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(NutritionPlanConsts.MaxDescriptionLength);

        b.Property(x => x.TargetCalories).HasColumnType("decimal(9,2)");
        b.Property(x => x.TargetProteinG).HasColumnType("decimal(9,2)");
        b.Property(x => x.TargetCarbsG).HasColumnType("decimal(9,2)");
        b.Property(x => x.TargetFatG).HasColumnType("decimal(9,2)");

        b.HasMany(x => x.Meals)
            .WithOne()
            .HasForeignKey(x => x.NutritionPlanTemplateId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.TenantId, x.Name });
    }
}
