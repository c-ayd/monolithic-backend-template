using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers.Authentication
{
    public partial class AuthenticationControllerTest
    {
        public const string _deleteLoginEndpoint = "/auth/logins/";

        [Fact]
        public async Task DeleteLogin_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_deleteLoginEndpoint + Guid.NewGuid());

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [InlineData("abcdefg")]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        public async Task DeleteLogin_WhenRouteParameterIsWrong_ShouldReturnBadRequest(string? id)
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_deleteLoginEndpoint + id);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task DeleteLogin_WhenLoginDoesNotExists_ShouldReturnNotFound()
        {
            // Arrange
            var user = new User()
            {
                Logins = new List<Login>()
                {
                    new Login() { RefreshTokenHashed = StringGenerator.GenerateUsingAsciiChars(10) }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_deleteLoginEndpoint + Guid.NewGuid());

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task DeleteLogin_WhenLoginDoesNotExists_ShouldReturnOkAndDeleteLogin()
        {
            // Arrange
            var user = new User()
            {
                Logins = new List<Login>()
                {
                    new Login() { RefreshTokenHashed = StringGenerator.GenerateUsingAsciiChars(10) }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var userId = user.Id;
            var loginId = user.Logins.ElementAt(0).Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_deleteLoginEndpoint + loginId);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var login = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.Id.Equals(loginId))
                .FirstOrDefaultAsync();
            Assert.Null(login);
        }
    }
}
