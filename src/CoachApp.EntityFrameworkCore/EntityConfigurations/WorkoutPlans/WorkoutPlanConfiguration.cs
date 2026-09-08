using CoachApp.Entites.WorkoutPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutPlans;

public class WorkoutPlanConfiguration : IEntityTypeConfiguration<WorkoutPlan>
{
    public void Configure(EntityTypeBuilder<WorkoutPlan> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutPlans", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(WorkoutPlanConsts.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(WorkoutPlanConsts.MaxDescriptionLength);

        b.HasMany(x => x.Days)
            .WithOne()
            .HasForeignKey(x => x.WorkoutPlanId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.TenantId, x.TraineeId });
    }
}
