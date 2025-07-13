using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Shared.ValueObjects;
using Template.Persistence.DbContexts;
using Template.Persistence.Repositories.UserManagement;
using Template.Test.Integration.Persistence.Collections;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.Fixtures.DbContexts;

namespace Template.Test.Integration.Persistence.Repositories.UserManagement
{
    [Collection(nameof(AppDbContextCollection))]
    public class UserRepositoryTest
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserRepository _userRepository;

        public UserRepositoryTest(AppDbContextFixture appDbContextFixture)
        {
            _appDbContext = appDbContextFixture.DbContext;

            _userRepository = new UserRepository(_appDbContext);
        }

        [Fact]
        public async Task AddAsync_WhenNewUserIsGiven_ShouldAddUser()
        {
            // Arrange
            var user = new User();

            // Act
            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            // Arrange
            var userId = user.Id;
            _appDbContext.UntrackEntity(user);
            var result = await _appDbContext.Users
                .Where(u => u.Id.Equals(userId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var user = new User();

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByEmailAsync_WhenUserExists_ShouldReturnUser()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var user = new User()
            {
                Email = email
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetByEmailAsync(EmailGenerator.Generate());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetIdByEmailAsync_WhenUserExists_ShouldReturnUserId()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var user = new User()
            {
                Email = email
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetIdByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result);
        }

        [Fact]
        public async Task GetIdByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetIdByEmailAsync(EmailGenerator.Generate());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetEmailByIdAsync_WhenUserExists_ShouldReturnEmail()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var user = new User()
            {
                Email = email
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetEmailByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result);
        }

        [Fact]
        public async Task GetEmailByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetEmailByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSecurityStateByIdAsync_WhenUserExists_ShouldReturnSecurityState()
        {
            // Arrange
            var user = new User()
            {
                SecurityState = new SecurityState()
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetSecurityStateByIdAsync(userId);

            // Arrange
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetSecurityStateByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetSecurityStateByIdAsync(Guid.NewGuid());

            // Arrange
            Assert.Null(result);
        }

        [Fact]
        public async Task GetSecurityStateByEmailAsync_WhenUserExists_ShouldReturnUserIdAndSecurityState()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var user = new User()
            {
                Email = email,
                SecurityState = new SecurityState()
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetSecurityStateByEmailAsync(email);

            // Arrange
            Assert.NotNull(result);

            var (id, securityState) = result.Value;
            Assert.Equal(userId, id);
            Assert.NotNull(securityState);
        }

        [Fact]
        public async Task GetSecurityStateByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetSecurityStateByEmailAsync(EmailGenerator.Generate());

            // Arrange
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserProfileByIdAsync_WhenUserExists_ShouldReturnUserProfile()
        {
            // Arrange
            var user = new User()
            {
                UserProfile = new UserProfile()
                {
                    Address = new Address()
                }
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetUserProfileByIdAsync(userId);

            // Arrange
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetUserProfileByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetUserProfileByIdAsync(Guid.NewGuid());

            // Arrange
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserProfileByEmailAsync_WhenUserExists_ShouldReturnUserIdAndUserProfile()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var user = new User()
            {
                Email = email,
                UserProfile = new UserProfile()
                {
                    Address = new Address()
                }
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetUserProfileByEmailAsync(email);

            // Arrange
            Assert.NotNull(result);

            var (id, userProfile) = result.Value;
            Assert.Equal(userId, id);
            Assert.NotNull(userProfile);
        }

        [Fact]
        public async Task GetUserProfileByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetUserProfileByEmailAsync(EmailGenerator.Generate());

            // Arrange
            Assert.Null(result);
        }

        [Fact]
        public async Task Delete_WhenUserIsGiven_ShouldSoftDeleteUser()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var user = new User();

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;

            // Act
            _userRepository.Delete(user);
            await _appDbContext.SaveChangesAsync();

            // Arrange
            _appDbContext.UntrackEntity(user);
            var result = await _appDbContext.Users
                .IgnoreQueryFilters()
                .Where(u => u.Id.Equals(userId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.True(result.IsDeleted, "The user is not marked as deleted.");
            Assert.True(result.DeletedDate >= startTime, "The deleted date is wrong.");
        }

        [Fact]
        public async Task GetRolesByIdAsync_WhenUserHasRoles_ShouldReturnAllRoles()
        {
            // Arrange
            var roles = new List<Role>()
            {
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) }
            };
            
            var user = new User();
            user.Roles = roles;

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetRolesByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roles.Count, result.Count);
        }

        [Fact]
        public async Task GetRolesByIdAsync_WhenUserHasNoRole_ShouldReturnEmptyList()
        {
            // Arrange
            var user = new User();
            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            var userId = user.Id;
            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetRolesByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRolesByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetRolesByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetRolesByEmailAsync_WhenUserHasRoles_ShouldReturnAllRoles()
        {
            // Arrange
            var roles = new List<Role>()
            {
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) }
            };

            var email = EmailGenerator.Generate();
            var user = new User()
            {
                Email = email
            };
            user.Roles = roles;

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetRolesByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roles.Count, result.Count);
        }

        [Fact]
        public async Task GetRolesByEmailAsync_WhenUserHasNoRole_ShouldReturnEmptyList()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var user = new User()
            {
                Email = email
            };

            await _userRepository.AddAsync(user);
            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntity(user);

            // Act
            var result = await _userRepository.GetRolesByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetRolesByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetRolesByEmailAsync(EmailGenerator.Generate());

            // Assert
            Assert.Null(result);
        }
    }
}
