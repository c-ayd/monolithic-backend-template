using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Template.Api.Utilities;
using Template.Application.Dtos.Controllers.Authentication;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers
{
    public partial class AuthenticationControllerTest
    {
        public const string _refreshTokenEndpoint = "/auth/refresh-token";

        [Fact]
        public async Task RefreshToken_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Act
            var result = await _testHostFixture.Client.PostAsync(_refreshTokenEndpoint, null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WhenTokenIsNotFound_ShouldReturnUnauthorized()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });
            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.SetCookies(new Dictionary<string, string>() { { CookieUtility.RefreshTokenKey, token.RefreshToken } });

            // Act
            var result = await _testHostFixture.Client.PostAsync(_refreshTokenEndpoint, null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WhenTokenIsExpired_ShouldReturnGoneAndDeleteOldLogin()
        {
            // Arrange
            var user = new User();

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.SetCookies(new Dictionary<string, string>() { { CookieUtility.RefreshTokenKey, token.RefreshToken } });

            user.Logins.Add(new Login()
            {
                RefreshTokenHashed = _hashing.HashSha256(token.RefreshToken),
                ExpirationDate = DateTime.UtcNow.AddDays(-1)
            });

            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.PostAsync(_refreshTokenEndpoint, null);

            // Assert
            Assert.Equal(HttpStatusCode.Gone, result.StatusCode);

            var logins = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.UserId.Equals(userId))
                .ToListAsync();
            Assert.Empty(logins);
        }

        [Fact]
        public async Task RefreshToken_WhenTokenIsValid_ShouldReturnOkWithCookiesAndAccessTokenAndUpdateCurrentLogin()
        {
            // Arrange
            var user = new User();

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.SetCookies(new Dictionary<string, string>() { { CookieUtility.RefreshTokenKey, token.RefreshToken } });

            user.Logins.Add(new Login()
            {
                RefreshTokenHashed = _hashing.HashSha256(token.RefreshToken),
                ExpirationDate = token.RefreshTokenExpirationDate
            });

            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.PostAsync(_refreshTokenEndpoint, null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.True(result.Headers.Contains(HeaderNames.SetCookie));

            var cookies = result.Headers.GetValues(HeaderNames.SetCookie);
            Assert.Single(cookies);

            var cookieDictionary = cookies.ElementAt(0)
                .Split(';')
                .Select(s => s.Split('='))
                .ToDictionary(kvp => kvp[0].Trim(), kvp => kvp.Length > 1 ? kvp[1].Trim() : null);
            Assert.NotNull(cookieDictionary[CookieUtility.RefreshTokenKey]);
            Assert.NotNull(cookieDictionary["expires"]);
            Assert.Contains("secure", cookieDictionary);
            Assert.Contains("httponly", cookieDictionary);
            Assert.Equal("/auth", cookieDictionary["path"]);
            Assert.Equal("none", cookieDictionary["samesite"]);

            var json = await result.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(json);
            var dataElement = jsonDocument.RootElement.GetProperty(JsonUtility.DataKey);
            var response = JsonSerializer.Deserialize<LoginDto>(dataElement.GetRawText(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(response);
            Assert.NotNull(response.AccessToken);

            var logins = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.UserId.Equals(userId))
                .ToListAsync();
            Assert.Single(logins);
        }
    }
}
