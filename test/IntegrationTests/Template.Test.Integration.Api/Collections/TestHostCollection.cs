using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Collections
{
    [CollectionDefinition(nameof(TestHostCollection))]
    public class TestHostCollection : ICollectionFixture<TestHostFixture>
    {
    }
}
