using CoachApp.Entites.WorkoutPlans;
using CoachApp.Entites.WorkoutPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutPlanTemplates;

public class WorkoutPlanTemplateConfiguration : IEntityTypeConfiguration<WorkoutPlanTemplate>
{
    public void Configure(EntityTypeBuilder<WorkoutPlanTemplate> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutPlanTemplates", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(WorkoutPlanConsts.MaxNameLength);
        b.Property(x => x.Description).HasMaxLength(WorkoutPlanConsts.MaxDescriptionLength);

        b.HasMany(x => x.Days)
            .WithOne()
            .HasForeignKey(x => x.WorkoutPlanTemplateId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => new { x.TenantId, x.Name });
    }
}
