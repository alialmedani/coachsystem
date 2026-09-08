using CoachApp.Entites.TrainingPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.TrainingPlans;

public class TrainingPlanConfiguration : IEntityTypeConfiguration<TrainingPlan>
{
    public void Configure(EntityTypeBuilder<TrainingPlan> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "TrainingPlans", CoachAppConsts.DbSchema);

        b.ConfigureByConvention(); // Id, audit, soft-delete, tenant, concurrency, extra props

        b.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(TrainingPlanConsts.MaxTitleLength);

        b.Property(x => x.Description)
            .HasMaxLength(TrainingPlanConsts.MaxDescriptionLength);

        b.Property(x => x.StartDate)
            .IsRequired();

        b.Property(x => x.EndDate)
            .IsRequired();

        // TraineeId references the Trainee aggregate by id only (no navigation
        // across aggregate roots); index it so per-trainee lookups stay cheap.
        b.HasIndex(x => new { x.TenantId, x.TraineeId });
        b.HasIndex(x => new { x.TenantId, x.Title });
    }
}
