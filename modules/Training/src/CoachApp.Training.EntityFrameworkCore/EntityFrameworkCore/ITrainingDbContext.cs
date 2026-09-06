using CoachApp.Training.Trainees;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace CoachApp.Training.EntityFrameworkCore;

/// <summary>
/// DbContext contract for the Training module. Repositories bind to this
/// interface (not the concrete context) so the host can route Training entities
/// into its own DbContext via <c>[ReplaceDbContext(typeof(ITrainingDbContext))]</c>.
/// </summary>
[ConnectionStringName(TrainingDbProperties.ConnectionStringName)]
public interface ITrainingDbContext : IEfCoreDbContext
{
    DbSet<Trainee> Trainees { get; }
}
