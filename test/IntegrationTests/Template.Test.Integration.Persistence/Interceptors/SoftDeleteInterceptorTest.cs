using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Interceptors
{
    [Collection(nameof(AppDbContextCollection))]
    public class SoftDeleteInterceptorTest
    {
        private readonly AppDbContext _appDbContext;

        public SoftDeleteInterceptorTest(AppDbContextFixture appDbContextFixture)
        {
            _appDbContext = appDbContextFixture.DbContext;
        }

        [Fact]
        public async Task SoftDeleteInterceptor_WhenEntityIsSoftDeleteableAndIsDeleted_ShouldNotDeleteEntityAndModifySoftDeleteProperties()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var user = new User();

            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;

            // Act
            _appDbContext.Users.Remove(user);
            await _appDbContext.SaveChangesAsync();

            // Assert
            _appDbContext.UntrackEntity(user);
            var result = await _appDbContext.Users
                .IgnoreQueryFilters()
                .Where(u => u.Id.Equals(userId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.True(result.IsDeleted, "The entity is not marked as deleted.");
            Assert.NotNull(result.DeletedDate);
            Assert.True(result.DeletedDate >= startTime, "The deleted date is wrong.");
        }
    }
}
