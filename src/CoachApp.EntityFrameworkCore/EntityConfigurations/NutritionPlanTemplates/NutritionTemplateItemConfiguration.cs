using CoachApp.Entites.NutritionPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionPlanTemplates;

public class NutritionTemplateItemConfiguration : IEntityTypeConfiguration<NutritionTemplateItem>
{
    public void Configure(EntityTypeBuilder<NutritionTemplateItem> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "NutritionTemplateItems", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Quantity).HasColumnType("decimal(9,2)");

        b.HasIndex(x => x.NutritionTemplateMealId);
        b.HasIndex(x => x.FoodId);
    }
}
