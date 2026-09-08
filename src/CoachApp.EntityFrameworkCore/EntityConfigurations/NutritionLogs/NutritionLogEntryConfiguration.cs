using CoachApp.Entites.NutritionLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionLogs;

public class NutritionLogEntryConfiguration : IEntityTypeConfiguration<NutritionLogEntry>
{
    public void Configure(EntityTypeBuilder<NutritionLogEntry> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "NutritionLogEntries", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Notes).HasMaxLength(NutritionLogConsts.MaxNotesLength);
        b.Property(x => x.Quantity).HasColumnType("decimal(9,2)");

        b.HasIndex(x => x.NutritionLogId);
        b.HasIndex(x => x.FoodId);
    }
}
