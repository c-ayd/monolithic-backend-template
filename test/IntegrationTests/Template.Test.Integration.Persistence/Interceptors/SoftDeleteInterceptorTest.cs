using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Interceptors
{
    [Collection(nameof(AppDbContextCollection))]
    public class SoftDeleteInterceptorTest
    {
        private readonly AppDbContext _dbContext;

        public SoftDeleteInterceptorTest(AppDbContextFixture appDbContextFixture)
        {
            _dbContext = appDbContextFixture.DbContext;
        }

        [Fact]
        public async Task SoftDeleteInterceptor_WhenEntityIsSoftDeleteableAndIsDeleted_ShouldNotDeleteEntityAndModifySoftDeleteProperties()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var user = new User();

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var userId = user.Id;

            // Act
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            // Assert
            _dbContext.Entry(user).State = EntityState.Unchanged;
            var result = await _dbContext.Users
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
