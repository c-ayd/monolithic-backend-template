using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Template.Application.Features.Commands.Admin.DeleteUser;
using Template.Application.Policies;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers.Admin
{
    public partial class AdminControllerTest
    {
        public const string _deleteUserEndpoint = "/admin/user/soft-delete";

        [Fact]
        public async Task DeleteUser_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new DeleteUserRequest()
            {
                Id = Guid.NewGuid()
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_deleteUserEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WhenUserIsNotAdmin_ShouldReturnForbidden()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "test-value")
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            var request = new DeleteUserRequest()
            {
                Id = Guid.NewGuid()
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_deleteUserEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task DeleteUser_WhenRouteParameterIsWrong_ShouldReturnBadRequest(string? id)
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new DeleteUserRequest()
            {
                Id = id != null ? new Guid(id) : null
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_deleteUserEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new DeleteUserRequest()
            {
                Id = Guid.NewGuid()
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_deleteUserEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task DeleteUser_WhenUserExists_ShouldReturnOkAndSoftDeleteUser()
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var user = new User()
            {
                Email = EmailGenerator.Generate(),
                SecurityState = new SecurityState()
                {
                    PasswordHashed = StringGenerator.GenerateUsingAsciiChars(10)
                },
                Logins = new List<Login>()
                {
                    new Login() { RefreshTokenHashed = StringGenerator.GenerateUsingAsciiChars(10) }
                },
                Tokens = new List<Token>()
                {
                    new Token() { ValueHashed = StringGenerator.GenerateUsingAsciiChars(10) }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user);

            var request = new DeleteUserRequest()
            {
                Id = userId
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_deleteUserEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var softDeletedUser = await _testHostFixture.AppDbContext.Users
                .IgnoreQueryFilters()
                .Where(u => u.Id.Equals(userId))
                .Include(u => u.SecurityState)
                .Include(u => u.Logins)
                .Include(u => u.Tokens)
                .FirstOrDefaultAsync();
            Assert.NotNull(softDeletedUser);
            Assert.Null(softDeletedUser.Email);
            Assert.True(softDeletedUser.IsDeleted, "The user is not marked as deleted.");
            Assert.NotNull(softDeletedUser.DeletedDate);
            Assert.Null(softDeletedUser.SecurityState);
            Assert.Empty(softDeletedUser.Logins);
            Assert.Empty(softDeletedUser.Tokens);
        }
    }
}
