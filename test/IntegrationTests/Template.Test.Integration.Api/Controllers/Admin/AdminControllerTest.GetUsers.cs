using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Template.Api.Policies;
using Template.Api.Utilities;
using Template.Application.Dtos.Controllers.Admin;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers.Admin
{
    public partial class AdminControllerTest
    {
        private const string _getUsersEndpoint = "/admin/users";

        [Fact]
        public async Task GetUsers_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUsersEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WhenUserIsNotAdmin_ShouldReturnForbidden()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUsersEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetUsers_WhenPageIsZeroOrNegative_ShouldReturnBadRequest(int page)
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUsersEndpoint + $"?page={page}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WhenPageSizeTooBig_ShouldReturnBadRequest()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUsersEndpoint + $"?page=1&pageSize={int.MaxValue}");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WhenThereIsNoUserInPage_ShouldReturnNoContent()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUsersEndpoint + $"?page=100000&pageSize=10");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task GetUsers_WhenPaginationIsNotGiven_ShouldReturnOkAndUsersAccordingToDefaultPagination()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            var users = new List<User>();
            for (int i = 0; i < 50; ++i)
            {
                users.Add(new User()
                {
                    SecurityState = new SecurityState()
                });
            }

            await _testHostFixture.AppDbContext.Users.AddRangeAsync(users);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            _testHostFixture.AppDbContext.UntrackEntities(users.ToArray());

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUsersEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var json = await result.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(json);
            var dataElement = jsonDocument.RootElement.GetProperty(JsonUtility.DataKey);
            var responseData = JsonSerializer.Deserialize<List<GetUserDto>>(dataElement.GetRawText(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(responseData);
            Assert.Equal(20, responseData.Count);

            var metadataElement = jsonDocument.RootElement.GetProperty(JsonUtility.MetadataKey);
            var responsePage = metadataElement.GetProperty("page").GetInt32();
            var responsePageSize = metadataElement.GetProperty("pageSize").GetInt32();
            var responseNumberOfNextPages = metadataElement.GetProperty("numberOfNextPages").GetInt32();
            Assert.Equal(1, responsePage);
            Assert.Equal(20, responsePageSize);
            Assert.True(responseNumberOfNextPages > 0, "The number of next pages is zero.");
        }

        [Fact]
        public async Task GetUsers_WhenPaginationIsGiven_ShouldReturnOkAndUsersAccordingToPagination()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            var users = new List<User>();
            for (int i = 0; i < 50; ++i)
            {
                users.Add(new User()
                {
                    SecurityState = new SecurityState()
                });
            }

            await _testHostFixture.AppDbContext.Users.AddRangeAsync(users);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            _testHostFixture.AppDbContext.UntrackEntities(users.ToArray());

            int page = 2;
            int pageSize = 15;

            // Act
            var result = await _testHostFixture.Client.GetAsync(_getUsersEndpoint + $"?page={page}&pageSize={pageSize}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var json = await result.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(json);
            var dataElement = jsonDocument.RootElement.GetProperty(JsonUtility.DataKey);
            var responseData = JsonSerializer.Deserialize<List<GetUserDto>>(dataElement.GetRawText(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(responseData);
            Assert.Equal(pageSize, responseData.Count);

            var metadataElement = jsonDocument.RootElement.GetProperty(JsonUtility.MetadataKey);
            var responsePage = metadataElement.GetProperty("page").GetInt32();
            var responsePageSize = metadataElement.GetProperty("pageSize").GetInt32();
            var responseNumberOfNextPages = metadataElement.GetProperty("numberOfNextPages").GetInt32();
            Assert.Equal(page, responsePage);
            Assert.Equal(pageSize, responsePageSize);
            Assert.True(responseNumberOfNextPages > 0, "The number of next pages is zero.");
        }
    }
}
