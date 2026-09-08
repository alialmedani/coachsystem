using CoachApp.Entites.Foods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.Foods;

public class FoodConfiguration : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "Foods", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(FoodConsts.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(FoodConsts.MaxDescriptionLength);
        b.Property(x => x.ServingUnit).IsRequired().HasMaxLength(FoodConsts.MaxServingUnitLength);

        b.Property(x => x.ServingSize).HasColumnType("decimal(9,2)");
        b.Property(x => x.Calories).HasColumnType("decimal(9,2)");
        b.Property(x => x.ProteinG).HasColumnType("decimal(7,2)");
        b.Property(x => x.CarbsG).HasColumnType("decimal(7,2)");
        b.Property(x => x.FatG).HasColumnType("decimal(7,2)");

        b.HasIndex(x => new { x.TenantId, x.Name });
    }
}
