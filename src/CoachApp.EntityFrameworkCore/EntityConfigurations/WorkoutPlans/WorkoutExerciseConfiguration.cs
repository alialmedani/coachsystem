using CoachApp.Entites.WorkoutPlans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.WorkoutPlans;

public class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "WorkoutExercises", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Reps).HasMaxLength(WorkoutPlanConsts.MaxRepsLength);
        b.Property(x => x.Notes).HasMaxLength(WorkoutPlanConsts.MaxNotesLength);
        b.Property(x => x.WeightKg).HasColumnType("decimal(6,2)");

        b.HasIndex(x => x.WorkoutDayId);
        b.HasIndex(x => x.ExerciseId);
    }
}
