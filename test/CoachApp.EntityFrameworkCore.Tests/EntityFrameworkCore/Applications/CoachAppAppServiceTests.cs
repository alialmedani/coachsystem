using CoachApp.Apis.Exercises;
using CoachApp.Apis.Foods;
using CoachApp.Apis.NutritionPlans;
using CoachApp.Apis.ProgressEntries;
using CoachApp.Apis.TraineeNotes;
using CoachApp.Apis.Trainees;
using CoachApp.Apis.WorkoutPlans;
using Xunit;

namespace CoachApp.EntityFrameworkCore.Applications;

/*
 * Concrete SQLite-backed runners for the coach-side application-service tests. The
 * test logic lives in the abstract classes under CoachApp.Application.Tests; these
 * bind them to the EF Core (in-memory SQLite) startup module and the shared test
 * collection, matching the repo's Sample-test convention.
 */

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreTraineeAppServiceTests : TraineeAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreExerciseAppServiceTests : ExerciseAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreFoodAppServiceTests : FoodAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreWorkoutPlanAppServiceTests : WorkoutPlanAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreNutritionPlanAppServiceTests : NutritionPlanAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreProgressEntryAppServiceTests : ProgressEntryAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreTraineeNoteAppServiceTests : TraineeNoteAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{
}
