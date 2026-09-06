using CoachApp.Training.Trainees;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace CoachApp.Training.EntityFrameworkCore;

[DependsOn(
    typeof(CoachAppTrainingDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class CoachAppTrainingEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<TrainingDbContext>(options =>
        {
            /* Repositories are registered against ITrainingDbContext so that the
             * host can replace the runtime DbContext with CoachAppDbContext. */
            options.AddDefaultRepositories<ITrainingDbContext>(includeAllEntities: true);

            options.AddRepository<Trainee, EfCoreTraineeRepository>();
        });
    }
}
