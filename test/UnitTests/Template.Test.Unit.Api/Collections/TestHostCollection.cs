using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Unit.Api.Collections
{
    [CollectionDefinition(nameof(TestHostCollection))]
    public class TestHostCollection : ICollectionFixture<TestHostFixture>
    {
    }
}
