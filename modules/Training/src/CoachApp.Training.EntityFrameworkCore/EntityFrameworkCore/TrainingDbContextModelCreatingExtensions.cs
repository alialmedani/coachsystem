using System;
using CoachApp.Training.Trainees;
using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace CoachApp.Training.EntityFrameworkCore;

/// <summary>
/// Model configuration for the Training module. Call this from any DbContext
/// that owns the Training tables (the standalone <see cref="TrainingDbContext"/>
/// and the host <c>CoachAppDbContext</c>) so the mapping is defined once.
/// </summary>
public static class TrainingDbContextModelCreatingExtensions
{
    public static void ConfigureTraining(
        this ModelBuilder builder,
        Action<TrainingModelBuilderConfigurationOptions>? optionsAction = null)
    {
        Check.NotNull(builder, nameof(builder));

        var options = new TrainingModelBuilderConfigurationOptions(
            TrainingDbProperties.DbTablePrefix,
            TrainingDbProperties.DbSchema
        );

        optionsAction?.Invoke(options);

        builder.Entity<Trainee>(b =>
        {
            b.ToTable(options.TablePrefix + "Trainees", options.Schema);

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
        });
    }
}
