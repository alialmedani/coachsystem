using CoachApp.Samples;
using Xunit;

namespace CoachApp.EntityFrameworkCore.Applications;

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<CoachAppEntityFrameworkCoreTestModule>
{

}
