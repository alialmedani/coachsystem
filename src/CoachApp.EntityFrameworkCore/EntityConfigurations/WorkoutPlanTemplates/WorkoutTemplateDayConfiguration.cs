using CoachApp.Entites.WorkoutPlans;
using CoachApp.Entites.WorkoutPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutPlanTemplates;

public class WorkoutTemplateDayConfiguration : IEntityTypeConfiguration<WorkoutTemplateDay>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplateDay> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutTemplateDays", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name).IsRequired().HasMaxLength(WorkoutPlanConsts.MaxDayNameLength);

        b.HasMany(x => x.Exercises)
            .WithOne()
            .HasForeignKey(x => x.WorkoutTemplateDayId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.WorkoutPlanTemplateId);
    }
}
