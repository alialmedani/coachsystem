using System.Threading.Tasks;

namespace CoachApp.Data;

public interface ICoachAppDbSchemaMigrator
{
    Task MigrateAsync();
}
