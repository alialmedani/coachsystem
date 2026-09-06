using Xunit;

namespace CoachApp.EntityFrameworkCore;

[CollectionDefinition(CoachAppTestConsts.CollectionDefinitionName)]
public class CoachAppEntityFrameworkCoreCollection : ICollectionFixture<CoachAppEntityFrameworkCoreFixture>
{

}
