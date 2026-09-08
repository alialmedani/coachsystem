using CoachApp.Entites.TraineeNotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.TraineeNotes;

public class TraineeNoteConfiguration : IEntityTypeConfiguration<TraineeNote>
{
    public void Configure(EntityTypeBuilder<TraineeNote> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "TraineeNotes", CoachAppConsts.DbSchema);

        b.ConfigureByConvention();

        b.Property(x => x.Text).IsRequired().HasMaxLength(TraineeNoteConsts.MaxTextLength);

        b.HasIndex(x => new { x.TenantId, x.TraineeId, x.Date });
    }
}
