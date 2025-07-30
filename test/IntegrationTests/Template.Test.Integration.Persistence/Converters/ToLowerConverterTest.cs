using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Converters
{
    [Collection(nameof(AppDbContextCollection))]
    public class ToLowerConverterTest
    {
        private readonly AppDbContext _appDbContext;

        public ToLowerConverterTest(AppDbContextFixture appDbContextFixture)
        {
            _appDbContext = appDbContextFixture.DbContext;
        }

        [Fact]
        public async Task ToLowerConverter_WhenValueIsGiven_ShouldLowerLettersBeforeSavingToDatabase()
        {
            // Arrange
            var email = EmailGenerator.Generate().ToUpper();
            var user = new User()
            {
                Email = email
            };

            // Act
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            // Assert
            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            var emailDb = await _appDbContext.Users
                .Where(u => u.Id.Equals(userId))
                .Select(u => u.Email)
                .FirstOrDefaultAsync();
            Assert.NotNull(emailDb);
            Assert.NotEqual(email, emailDb);
            Assert.Equal(email.ToLower(), emailDb);
        }
    }
}
