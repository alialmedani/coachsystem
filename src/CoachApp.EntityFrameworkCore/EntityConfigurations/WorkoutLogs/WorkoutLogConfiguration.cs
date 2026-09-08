using CoachApp.Entites.WorkoutLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutLogs;

public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutLogs", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Notes).HasMaxLength(WorkoutLogConsts.MaxNotesLength);

        b.HasMany(x => x.Entries)
            .WithOne()
            .HasForeignKey(x => x.WorkoutLogId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.TenantId, x.TraineeId, x.Date });
    }
}
