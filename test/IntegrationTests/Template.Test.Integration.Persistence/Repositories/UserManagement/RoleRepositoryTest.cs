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
    public class RoleRepositoryTest
    {
        private readonly AppDbContext _appDbContext;
        private readonly RoleRepository _roleRepository;

        public RoleRepositoryTest(AppDbContextFixture appDbContextFixture)
        {
            _appDbContext = appDbContextFixture.DbContext;

            _roleRepository = new RoleRepository(_appDbContext);
        }

        [Fact]
        public async Task AddAsync_WhenNewRoleIsGiven_ShouldAddRole()
        {
            // Arrange
            var role = new Role()
            {
                Name = StringGenerator.GenerateUsingAsciiChars(10)
            };

            // Act
            await _roleRepository.AddAsync(role);
            await _appDbContext.SaveChangesAsync();

            // Assert
            var roleId = role.Id;
            _appDbContext.UntrackEntity(role);
            var result = await _appDbContext.Roles
                .Where(u => u.Id.Equals(roleId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRoleExists_ShouldReturnRole()
        {
            // Arrange
            var role = new Role()
            {
                Name = StringGenerator.GenerateUsingAsciiChars(10)
            };

            await _roleRepository.AddAsync(role);
            await _appDbContext.SaveChangesAsync();

            var roleId = role.Id;
            _appDbContext.UntrackEntity(role);

            // Act
            var result = await _roleRepository.GetByIdAsync(roleId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRoleDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _roleRepository.GetByIdAsync(int.MaxValue);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByNameAsync_WhenRoleExists_ShouldReturnRole()
        {
            // Arrange
            var name = StringGenerator.GenerateUsingAsciiChars(10);
            var role = new Role()
            {
                Name = name
            };

            await _roleRepository.AddAsync(role);
            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntity(role);

            // Act
            var result = await _roleRepository.GetByNameAsync(name);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetByNameAsync_WhenRoleDoesNotExist_ShouldReturnNull()
        {
            // Act
            var result = await _roleRepository.GetByNameAsync(StringGenerator.GenerateUsingAsciiChars(5));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllAsync_WhenPaginationIsCorrect_ShouldReturnRoles()
        {
            // Arrange
            var roles = new List<Role>()
            {
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) }
            };

            foreach (var role in roles)
            {
                await _roleRepository.AddAsync(role);
            }

            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntities(roles.ToArray());

            int page = 1;
            int pageSize = 3;

            // Act
            var result = await _roleRepository.GetAllAsync(page, pageSize);

            // Assert
            Assert.Equal(pageSize, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_WhenPaginationExceedsLimits_ShouldReturnEmptyList()
        {
            // Arrange
            var roles = new List<Role>()
            {
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) }
            };

            foreach (var role in roles)
            {
                await _roleRepository.AddAsync(role);
            }

            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntities(roles.ToArray());

            int page = 10;
            int pageSize = 10;

            // Act
            var result = await _roleRepository.GetAllAsync(page, pageSize);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WhenPaginationIsWrong_ShouldReturnEmptyList()
        {
            // Arrange
            var roles = new List<Role>()
            {
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) },
                new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) }
            };

            foreach (var role in roles)
            {
                await _roleRepository.AddAsync(role);
            }

            await _appDbContext.SaveChangesAsync();

            _appDbContext.UntrackEntities(roles.ToArray());

            int page = 0;
            int pageSize = -5;

            // Act
            var result = await _roleRepository.GetAllAsync(page, pageSize);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Delete_WhenRoleIsGiven_ShouldSoftDeleteRole()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var role = new Role()
            {
                Name = StringGenerator.GenerateUsingAsciiChars(10)
            };

            await _roleRepository.AddAsync(role);
            await _appDbContext.SaveChangesAsync();

            var roleId = role.Id;

            // Act
            _roleRepository.Delete(role);
            await _appDbContext.SaveChangesAsync();

            // Arrange
            _appDbContext.UntrackEntity(role);
            var result = await _appDbContext.Roles
                .IgnoreQueryFilters()
                .Where(u => u.Id.Equals(roleId))
                .FirstOrDefaultAsync();

            Assert.NotNull(result);
            Assert.True(result.IsDeleted, "The role is not marked as deleted.");
            Assert.True(result.DeletedDate >= startTime, "The deleted date is wrong.");
        }
    }
}
