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
        public async Task GetByUserIdAndRefreshTokenAsync_WhenUserIdAndRefreshTokenExist_ShouldReturnLogin()
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

            var userId = user.Id;
            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(login);

            // Act
            var result = await _loginRepository.GetByUserIdAndRefreshTokenAsync(userId, refreshToken);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByUserIdAndRefreshTokenAsync_WhenUserIdExistsButRefreshTokenDoesNotExist_ShouldReturnNull()
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

            var userId = user.Id;
            _appDbContext.UntrackEntities(user.Logins.ToArray());
            _appDbContext.UntrackEntity(user);
            _appDbContext.UntrackEntity(login);

            // Act
            var result = await _loginRepository.GetByUserIdAndRefreshTokenAsync(userId, StringGenerator.GenerateUsingAsciiChars(10));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAndRefreshTokenAsync_WhenUserIdDoesNotExistButRefreshTokenExists_ShouldReturnNull()
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
            var result = await _loginRepository.GetByUserIdAndRefreshTokenAsync(Guid.NewGuid(), refreshToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByUserIdAndRefreshTokenAsync_WhenUserIdAndRefreshTokenDoNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _loginRepository.GetByUserIdAndRefreshTokenAsync(Guid.NewGuid(), StringGenerator.GenerateUsingAsciiChars(10));

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

        [Fact]
        public async Task DeleteAllByUserIdAsync_WhenLoginsRelatedToUserExist_ShouldDeleteAllLoginsRelatedToUser()
        {
            // Arrange
            var user1 = new User()
            {
                Logins = new List<Login>()
                {
                    new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                    new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                    new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) }
                }
            };
            var user2 = new User()
            {
                Logins = new List<Login>()
                {
                    new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                    new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) },
                    new Login() { RefreshToken = StringGenerator.GenerateUsingAsciiChars(10) }
                }
            };

            await _appDbContext.Users.AddAsync(user1);
            await _appDbContext.Users.AddAsync(user2);
            await _appDbContext.SaveChangesAsync();

            var userId1 = user1.Id;
            var userId2 = user2.Id;
            _appDbContext.UntrackEntities(user1.Logins.ToArray());
            _appDbContext.UntrackEntity(user1);
            _appDbContext.UntrackEntities(user2.Logins.ToArray());
            _appDbContext.UntrackEntity(user2);

            // Act
            var result = await _loginRepository.DeleteAllByUserIdAsync(userId1);

            // Assert
            Assert.Equal(user1.Logins.Count, result);

            var user1Logins = await _appDbContext.Logins
                .Where(l => l.UserId.Equals(userId1))
                .ToListAsync();
            Assert.Empty(user1Logins);

            var user2Logins = await _appDbContext.Logins
                .Where(l => l.UserId.Equals(userId2))
                .ToListAsync();
            Assert.Equal(user2.Logins.Count, user2Logins.Count);
        }

        [Fact]
        public async Task DeleteAllByUserIdAsync_WhenLoginsRelatedToUserDoNotExist_ShouldDeleteNothing()
        {
            // Act
            var result = await _loginRepository.DeleteAllByUserIdAsync(Guid.NewGuid());

            // Assert
            Assert.Equal(0, result);
        }
    }
}
