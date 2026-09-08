using CoachApp.Entites.Trainees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.EntityConfigurations.Trainees;

public class TraineeConfiguration : IEntityTypeConfiguration<Trainee>
{
    public void Configure(EntityTypeBuilder<Trainee> b)
    {
        b.ToTable(CoachAppConsts.DbTablePrefix + "Trainees", CoachAppConsts.DbSchema);

        b.ConfigureByConvention(); // Id, audit, soft-delete, tenant, concurrency, extra props

        b.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(TraineeConsts.MaxCodeLength);

        b.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(TraineeConsts.MaxFirstNameLength);

        b.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(TraineeConsts.MaxLastNameLength);

        b.Property(x => x.Gender)
            .HasConversion<byte>();

        b.Property(x => x.Email)
            .HasMaxLength(TraineeConsts.MaxEmailLength);

        b.Property(x => x.PhoneNumber)
            .HasMaxLength(TraineeConsts.MaxPhoneNumberLength);

        b.Property(x => x.Address)
            .HasMaxLength(TraineeConsts.MaxAddressLength);

        b.HasIndex(x => new { x.TenantId, x.Code });
    }
}
