using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Collections
{
    [CollectionDefinition(nameof(AppDbContextCollection))]
    public class AppDbContextCollection : ICollectionFixture<AppDbContextFixture>
    {
    }
}
