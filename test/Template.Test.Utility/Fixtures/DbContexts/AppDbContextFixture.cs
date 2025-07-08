using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Template.Persistence.DbContexts;
using Template.Persistence.Settings;

namespace Template.Test.Utility.Fixtures.DbContexts
{
    public class AppDbContextFixture : IAsyncLifetime
    {
        public IConfiguration Configuration { get; private set; } = null!;
        public AppDbContext DbContext { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Configuration = ConfigurationHelper.CreateConfiguration();

            var connectionStrings = Configuration.GetSection(ConnectionStringsSettings.SettingsKey).Get<ConnectionStringsSettings>()!;
            var dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionStrings.App)
                .Options;

            DbContext = new AppDbContext(dbContextOptions);

            if (await DbContext.Database.CanConnectAsync())
            {
                await DbContext.Database.EnsureCreatedAsync();
            }

            await DbContext.Database.MigrateAsync();
        }

        public async Task DisposeAsync()
        {
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.DisposeAsync();
        }
    }
}
