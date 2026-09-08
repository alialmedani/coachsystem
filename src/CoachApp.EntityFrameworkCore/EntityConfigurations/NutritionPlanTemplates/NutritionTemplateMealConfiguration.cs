using CoachApp.Entites.NutritionPlans;
using CoachApp.Entites.NutritionPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionPlanTemplates;

public class NutritionTemplateMealConfiguration : IEntityTypeConfiguration<NutritionTemplateMeal>
{
    public void Configure(EntityTypeBuilder<NutritionTemplateMeal> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "NutritionTemplateMeals", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(NutritionPlanConsts.MaxMealNameLength);

        b.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.NutritionTemplateMealId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.NutritionPlanTemplateId);
    }
}
