using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Interceptors
{
    [Collection(nameof(AppDbContextCollection))]
    public class UpdatedDateInterceptorTest
    {
        private readonly AppDbContext _appDbContext;

        public UpdatedDateInterceptorTest(AppDbContextFixture appDbContextFixture)
        {
            _appDbContext = appDbContextFixture.DbContext;
        }

        [Fact]
        public async Task UpdatedDateInterceptor_WhenEntityIsUpdated_ShouldAlsoUpdateUpdatedDate()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var user = new User();

            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;

            // Act
            user.Email = EmailGenerator.Generate();
            await _appDbContext.SaveChangesAsync();

            // Assert
            _appDbContext.UntrackEntity(user);
            var result = await _appDbContext.Users
                .Where(u => u.Id.Equals(userId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.NotNull(result.UpdatedDate);
            Assert.True(result.UpdatedDate >= startTime, "The updated date is wrong.");
        }
    }
}
