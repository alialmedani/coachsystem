using CoachApp.Entites.WorkoutPlans;
using CoachApp.Entites.WorkoutPlanTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutPlanTemplates;

public class WorkoutTemplateExerciseConfiguration : IEntityTypeConfiguration<WorkoutTemplateExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplateExercise> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutTemplateExercises", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Reps).HasMaxLength(WorkoutPlanConsts.MaxRepsLength);
        b.Property(x => x.Notes).HasMaxLength(WorkoutPlanConsts.MaxNotesLength);
        b.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");

        b.HasIndex(x => x.WorkoutTemplateDayId);
        b.HasIndex(x => x.ExerciseId);
    }
}
