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

        b.Property(x => x.UserName)
            .IsRequired()
            .HasMaxLength(TraineeConsts.MaxUserNameLength);

        b.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(TraineeConsts.MaxFirstNameLength);

        b.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(TraineeConsts.MaxLastNameLength);

        b.Property(x => x.Email)
            .HasMaxLength(TraineeConsts.MaxEmailLength);

        b.Property(x => x.PhoneNumber)
            .HasMaxLength(TraineeConsts.MaxPhoneNumberLength);

        b.Property(x => x.Gender).HasConversion<byte>();
        b.Property(x => x.Goal).HasConversion<byte>();

        b.Property(x => x.HeightCm).HasColumnType("decimal(5,2)");
        b.Property(x => x.StartWeightKg).HasColumnType("decimal(6,2)");
        b.Property(x => x.TargetWeightKg).HasColumnType("decimal(6,2)");

        // One profile per login user; index tenant-scoped lookups.
        b.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.UserName });
    }
}
