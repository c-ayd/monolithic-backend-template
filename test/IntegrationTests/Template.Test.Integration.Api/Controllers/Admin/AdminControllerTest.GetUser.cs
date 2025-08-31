using Cayd.Test.Generators;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Template.Api.Utilities;
using Template.Application.Dtos.Controllers.Admin;
using Template.Application.Policies;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers.Admin
{
    public partial class AdminControllerTest
    {
        public const string _getUserEndpoint = "/admin/user/";

        [Fact]
        public async Task GetUser_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUserEndpoint + Guid.NewGuid());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetUser_WhenUserIsNotAdmin_ShouldReturnForbidden()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "test-value")
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUserEndpoint + Guid.NewGuid());

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task GetUser_WhenRouteParameterIsWrong_ShouldReturnBadRequest(string? id)
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUserEndpoint + id);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetUser_WhenUserDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUserEndpoint + Guid.NewGuid());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetUser_WhenUserIsSoftDeleted_ShouldRetunOkAndReturnOnlyUserContext()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            var user = new User()
            {
                Email = EmailGenerator.Generate()
            };
            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();
            _testHostFixture.AppDbContext.Users.Remove(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUserEndpoint + userId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var json = await result.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(json);
            var dataElement = jsonDocument.RootElement.GetProperty(JsonUtility.DataKey);
            var responseData = JsonSerializer.Deserialize<GetUserDto>(dataElement.GetRawText(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(responseData);
            Assert.Null(responseData.SecurityState);
            Assert.Empty(responseData.Roles);
            Assert.Empty(responseData.Logins);
        }

        [Fact]
        public async Task GetUser_WhenUserExists_ShouldReturnOkAndReturnUserWithFullContext()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            var user = new User()
            {
                Email = EmailGenerator.Generate(),
                SecurityState = new SecurityState(),
                Roles = new List<Role>()
                {
                    new Role() { Name = StringGenerator.GenerateUsingAsciiChars(10) }
                },
                Logins = new List<Login>()
                {
                    new Login() { RefreshTokenHashed = StringGenerator.GenerateUsingAsciiChars(10) }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Roles.ToArray());
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUserEndpoint + userId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var json = await result.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(json);
            var dataElement = jsonDocument.RootElement.GetProperty(JsonUtility.DataKey);
            var responseData = JsonSerializer.Deserialize<GetUserDto>(dataElement.GetRawText(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(responseData);
            Assert.NotNull(responseData.SecurityState);
            Assert.NotEmpty(responseData.Roles);
            Assert.NotEmpty(responseData.Logins);
        }
    }
}
