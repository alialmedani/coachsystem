using CoachApp.Samples;
using Xunit;

namespace CoachApp.EntityFrameworkCore.Domains;

[Collection(CoachAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<CoachAppEntityFrameworkCoreTestModule>
{

}
