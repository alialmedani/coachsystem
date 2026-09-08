using CoachApp.Entites.ProgressEntries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.ProgressEntries;

public class ProgressEntryConfiguration : IEntityTypeConfiguration<ProgressEntry>
{
    public void Configure(EntityTypeBuilder<ProgressEntry> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "ProgressEntries", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");
        b.Property(x => x.BodyFatPercent).HasColumnType("decimal(5,2)");
        b.Property(x => x.ChestCm).HasColumnType("decimal(6,2)");
        b.Property(x => x.WaistCm).HasColumnType("decimal(6,2)");
        b.Property(x => x.HipsCm).HasColumnType("decimal(6,2)");
        b.Property(x => x.ArmCm).HasColumnType("decimal(6,2)");
        b.Property(x => x.ThighCm).HasColumnType("decimal(6,2)");
        b.Property(x => x.Notes).HasMaxLength(ProgressEntryConsts.MaxNotesLength);

        b.HasIndex(x => new { x.TenantId, x.TraineeId, x.Date });
    }
}
