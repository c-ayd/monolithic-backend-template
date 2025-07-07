using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Interceptors
{
    [Collection(nameof(AppDbContextCollection))]
    public class CreatedDateInterceptorTest
    {
        private readonly AppDbContext _dbContext;

        public CreatedDateInterceptorTest(AppDbContextFixture appDbContextFixture)
        {
            _dbContext = appDbContextFixture.DbContext;
        }

        [Fact]
        public async Task CreatedDateInterceptor_WhenNewEntityIsAddedToDb_ShouldAlsoAddCreatedDateToEntity()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var user = new User();

            // Act
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var userId = user.Id;

            // Assert
            _dbContext.Entry(user).State = EntityState.Unchanged;
            var result = await _dbContext.Users
                .Where(u => u.Id.Equals(userId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.True(result.CreatedDate >= startTime, "The created date is wrong.");
        }
    }
}
