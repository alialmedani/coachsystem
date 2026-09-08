using CoachApp.Entites.Exercises;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.Exercises;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "Exercises", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(ExerciseConsts.MaxNameLength);

        b.Property(x => x.Description).HasMaxLength(ExerciseConsts.MaxDescriptionLength);
        b.Property(x => x.Instructions).HasMaxLength(ExerciseConsts.MaxInstructionsLength);
        b.Property(x => x.VideoUrl).HasMaxLength(ExerciseConsts.MaxMediaUrlLength);
        b.Property(x => x.ImageUrl).HasMaxLength(ExerciseConsts.MaxMediaUrlLength);

        b.Property(x => x.TargetMuscle).HasConversion<byte>();
        b.Property(x => x.Equipment).HasConversion<byte>();

        b.HasIndex(x => new { x.TenantId, x.Name });
        b.HasIndex(x => new { x.TenantId, x.TargetMuscle });
    }
}
