using CoachApp.Entites.WorkoutLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutLogs;

public class WorkoutLogEntryConfiguration : IEntityTypeConfiguration<WorkoutLogEntry>
{
    public void Configure(EntityTypeBuilder<WorkoutLogEntry> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutLogEntries", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Reps).HasMaxLength(WorkoutLogConsts.MaxRepsLength);
        b.Property(x => x.Notes).HasMaxLength(WorkoutLogConsts.MaxNotesLength);
        b.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");

        b.Property(x => x.PrescribedReps).HasMaxLength(WorkoutLogConsts.MaxRepsLength);
        b.Property(x => x.PrescribedWeightKg).HasColumnType("decimal(6,2)");

        b.HasIndex(x => x.WorkoutLogId);
        b.HasIndex(x => x.ExerciseId);
    }
}
