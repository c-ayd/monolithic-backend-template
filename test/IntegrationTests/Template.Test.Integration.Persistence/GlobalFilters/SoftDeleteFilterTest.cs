using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.GlobalFilters
{
    [Collection(nameof(AppDbContextCollection))]
    public class SoftDeleteFilterTest
    {
        private readonly AppDbContext _dbContext;

        public SoftDeleteFilterTest(AppDbContextFixture appDbContextFixture)
        {
            _dbContext = appDbContextFixture.DbContext;
        }

        [Fact]
        public async Task SoftDeleteFilter_WhenEntityIsSoftDeleteableAndIsDeleted_ShouldNotAppearInResult()
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
                .Where(u => u.Id.Equals(userId))
                .FirstOrDefaultAsync();

            Assert.Null(result);
        }
    }
}
