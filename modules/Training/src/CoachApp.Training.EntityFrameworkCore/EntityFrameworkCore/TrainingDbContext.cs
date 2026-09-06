using CoachApp.Training.Trainees;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace CoachApp.Training.EntityFrameworkCore;

/// <summary>
/// Standalone DbContext for the Training module. Used when the module runs in
/// isolation (e.g. tests). In the host application the entities are served by
/// <c>CoachAppDbContext</c>, which is registered as a replacement for
/// <see cref="ITrainingDbContext"/>.
/// </summary>
[ConnectionStringName(TrainingDbProperties.ConnectionStringName)]
public class TrainingDbContext : AbpDbContext<TrainingDbContext>, ITrainingDbContext
{
    public DbSet<Trainee> Trainees { get; set; } = null!;

    public TrainingDbContext(DbContextOptions<TrainingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureTraining();
    }
}
