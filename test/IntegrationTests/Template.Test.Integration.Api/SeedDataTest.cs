using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Template.Application.Policies;
using Template.Persistence.Settings;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api
{
    [Collection(nameof(TestHostCollection))]
    public class SeedDataTest
    {
        private readonly TestHostFixture _testHostFixture;

        public SeedDataTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
        }

        [Fact]
        public async Task AppDbContext_WhenApplicationStarts_ShouldSeedData()
        {
            // Arrange
            var seedDataSettings = _testHostFixture.Configuration.GetSection(SeedDataAppDbContextSettings.SettingsKey).Get<SeedDataAppDbContextSettings>()!;

            var adminEmails = seedDataSettings.AdminEmails
                .OrderBy(e => e)
                .ToList();

            // Assert
            var adminRole = await _testHostFixture.AppDbContext.Roles
                .Where(r => r.Name == AdminPolicy.RoleName)
                .FirstOrDefaultAsync();
            Assert.NotNull(adminRole);

            var emails = await _testHostFixture.AppDbContext.Users
                .Where(u => u.Email != null && adminEmails.Contains(u.Email))
                .Select(u => u.Email)
                .OrderBy(e => e)
                .ToListAsync();
            Assert.NotEmpty(emails);
            Assert.Equal(adminEmails, emails!);
        }
    }
}
