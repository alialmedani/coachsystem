using CoachApp.Apis.Dashboards;
using CoachApp.Apis.NutritionPlanTemplates;
using CoachApp.Apis.WorkoutPlanTemplates;
using Xunit;

namespace CoachApp.EntityFrameworkCore.Applications;

/*
 * Concrete SQLite-backed runners for the template + dashboard application-service tests.
 * The test logic lives in the abstract classes under CoachApp.Application.Tests; these bind
 * them to the EF Core startup module and the shared test collection, matching the repo convention.
 */

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreWorkoutPlanTemplateAppServiceTests : WorkoutPlanTemplateAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreNutritionPlanTemplateAppServiceTests : NutritionPlanTemplateAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreTraineeDashboardAppServiceTests : TraineeDashboardAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}
