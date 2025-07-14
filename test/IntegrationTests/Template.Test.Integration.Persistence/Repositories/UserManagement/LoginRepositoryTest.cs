using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Persistence.DbContexts;
using Template.Persistence.Repositories.UserManagement;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Repositories.UserManagement
{
    [Collection(nameof(AppDbContextCollection))]
    public class LoginRepositoryTest
    {
        private readonly AppDbContext _appDbContext;

        private readonly LoginRepository _loginRepository;

        public LoginRepositoryTest(AppDbContextFixture appDbContextFixture)
        {
            _appDbContext = appDbContextFixture.DbContext;

            _loginRepository = new LoginRepository(_appDbContext);
        }

        [Fact]
        public async Task AddAsync_WhenLoginIsGiven_ShouldAddLogin()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var login = new Login()
            {
                RefreshToken = StringGenerator.GenerateUsingAsciiChars(10),
                UserId = user.Id
            };

            // Act
            await _loginRepository.AddAsync(login);
            await _appDbContext.SaveChangesAsync();

            // Assert
            var loginId = login.Id;
            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(login);
            var result = await _appDbContext.Logins
                .Where(l => l.Id.Equals(loginId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenLoginExists_ShouldReturnLogin()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var login = new Login()
            {
                RefreshToken = StringGenerator.GenerateUsingAsciiChars(10),
                UserId = user.Id
            };
            await _loginRepository.AddAsync(login);
            await _appDbContext.SaveChangesAsync();

            var loginId = login.Id;
            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(login);

            // Act
            var result = await _loginRepository.GetByIdAsync(loginId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenLoginDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _loginRepository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByRefreshTokenAsync_WhenRefreshTokenExists_ShouldReturnLogin()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var refreshToken = StringGenerator.GenerateUsingAsciiChars(10);
            var login = new Login()
            {
                RefreshToken = refreshToken,
                UserId = user.Id
            };
            await _loginRepository.AddAsync(login);
            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(login);

            // Act
            var result = await _loginRepository.GetByRefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByRefreshTokenAsync_WhenRefreshTokenDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _loginRepository.GetByRefreshTokenAsync(StringGenerator.GenerateUsingAsciiChars(10));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_WhenLoginsRelatedToUserExist_ShouldReturnAllLogins()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user.Logins = new List<Login>()
            {
                new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) }
            };

            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _loginRepository.GetAllByUserIdAsync(userId);

            // Assert
            Assert.Equal(user.Logins.Count, result.Count);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_WhenLoginsRelatedToUserDoNotExist_ShouldReturnEmptyCollection()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _loginRepository.GetAllByUserIdAsync(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllByUserIdAsync_WhenUserDoesNotExist_ShouldReturnEmptyCollection()
        {
            // Act
            var result = await _loginRepository.GetAllByUserIdAsync(Guid.NewGuid());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Delete_WhenLoginExists_ShouldDeleteLogin()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var login = new Login()
            {
                RefreshToken = StringGenerator.GenerateUsingAsciiChars(10),
                UserId = user.Id
            };
            await _loginRepository.AddAsync(login);
            await _appDbContext.SaveChangesAsync();

            var loginId = login.Id;
            
            // Act
            _loginRepository.Delete(login);
            await _appDbContext.SaveChangesAsync();

            // Assert
            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(login);
            var result = await _appDbContext.Logins
                .Where(l => l.Id.Equals(loginId))
                .FirstOrDefaultAsync();

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAll_WhenLoginsExist_ShouldDeleteLogins()
        {
            // Arrange
            var user = new User();
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            user.Logins = new List<Login>()
            {
                new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) }
            };

            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;

            // Act
            _loginRepository.DeleteAll(user.Logins);
            await _appDbContext.SaveChangesAsync();

            // Assert
            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);
            var result = await _appDbContext.Logins
                .Where(l => l.UserId.Equals(userId))
                .ToListAsync();

            Assert.Empty(result);
        }
    }
}
