using CoachApp.Entites.NutritionLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.NutritionLogs;

public class NutritionLogConfiguration : IEntityTypeConfiguration<NutritionLog>
{
    public void Configure(EntityTypeBuilder<NutritionLog> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "NutritionLogs", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Notes).HasMaxLength(NutritionLogConsts.MaxNotesLength);

        b.HasMany(x => x.Entries)
            .WithOne()
            .HasForeignKey(x => x.NutritionLogId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.TenantId, x.TraineeId, x.Date });
    }
}
