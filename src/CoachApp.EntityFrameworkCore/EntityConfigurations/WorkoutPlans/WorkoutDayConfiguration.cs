using CoachApp.Entites.WorkoutPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutPlans;

public class WorkoutDayConfiguration : IEntityTypeConfiguration<WorkoutDay>
{
    public void Configure(EntityTypeBuilder<WorkoutDay> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutDays", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(WorkoutPlanConsts.MaxDayNameLength);

        b.Property(x => x.ScheduledDay).HasConversion<byte?>();

        b.HasMany(x => x.Exercises)
            .WithOne()
            .HasForeignKey(x => x.WorkoutDayId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.WorkoutPlanId);
        b.HasIndex(x => new { x.WorkoutPlanId, x.ScheduledDay });
    }
}
