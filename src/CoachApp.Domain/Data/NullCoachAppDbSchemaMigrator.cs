using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace CoachApp.Data;

/* This is used if database provider does't define
 * ICoachAppDbSchemaMigrator implementation.
 */
public class NullCoachAppDbSchemaMigrator : ICoachAppDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
