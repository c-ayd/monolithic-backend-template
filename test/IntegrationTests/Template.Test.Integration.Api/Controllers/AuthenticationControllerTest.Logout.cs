using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using Template.Api.Utilities;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers
{
    public partial class AuthenticationControllerTest
    {
        private const string _logoutEndpoint = "/auth/logout";

        [Fact]
        public async Task Logout_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_logoutEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Fact]
        public async Task Logout_WhenLoggedInButRefreshTokenIsMissing_ShouldReturnUnauthorized()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var user = new User()
            {
                Email = email,
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(password)
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            var login = new Login()
            {
                RefreshToken = token.RefreshToken,
                ExpirationDate = token.RefreshTokenExpirationDate,
                UserId = user.Id
            };

            await _testHostFixture.AppDbContext.Logins.AddAsync(login);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_logoutEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task Logout_WhenLoggedInAndNoQueryStringIsGiven_ShouldDeleteOnlyCurrentLogin()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var user = new User()
            {
                Email = email,
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(password)
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var token1 = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            var token2 = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            var login1 = new Login()
            {
                RefreshToken = token1.RefreshToken,
                ExpirationDate = token1.RefreshTokenExpirationDate,
                UserId = user.Id
            };
            var login2 = new Login()
            {
                RefreshToken = token2.RefreshToken,
                ExpirationDate = token2.RefreshTokenExpirationDate,
                UserId = user.Id
            };

            await _testHostFixture.AppDbContext.Logins.AddAsync(login1);
            await _testHostFixture.AppDbContext.Logins.AddAsync(login2);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            _testHostFixture.AppDbContext.UntrackEntity(login1);
            _testHostFixture.AppDbContext.UntrackEntity(login2);

            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.SetCookies(new Dictionary<string, string>() { { CookieUtility.RefreshTokenKey, token1.RefreshToken } });

            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_logoutEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var logins = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.UserId.Equals(user.Id))
                .ToListAsync();
            Assert.Single(logins);
            Assert.NotEqual(token1.RefreshToken, logins[0].RefreshToken);

            _testHostFixture.RemoveJwtBearerToken();
            _testHostFixture.ClearCookies();
        }

        [Fact]
        public async Task Logout_WhenLoggedInAndLogoutAllDevicesIsFalse_ShouldDeleteOnlyCurrentLogin()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var user = new User()
            {
                Email = email,
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(password)
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var token1 = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            var token2 = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            var login1 = new Login()
            {
                RefreshToken = token1.RefreshToken,
                ExpirationDate = token1.RefreshTokenExpirationDate,
                UserId = user.Id
            };
            var login2 = new Login()
            {
                RefreshToken = token2.RefreshToken,
                ExpirationDate = token2.RefreshTokenExpirationDate,
                UserId = user.Id
            };

            await _testHostFixture.AppDbContext.Logins.AddAsync(login1);
            await _testHostFixture.AppDbContext.Logins.AddAsync(login2);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            _testHostFixture.AppDbContext.UntrackEntity(login1);
            _testHostFixture.AppDbContext.UntrackEntity(login2);

            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.SetCookies(new Dictionary<string, string>() { { CookieUtility.RefreshTokenKey, token1.RefreshToken } });

            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_logoutEndpoint + "?logoutAllDevices=false");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var logins = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.UserId.Equals(user.Id))
                .ToListAsync();
            Assert.Single(logins);
            Assert.NotEqual(token1.RefreshToken, logins[0].RefreshToken);

            _testHostFixture.RemoveJwtBearerToken();
            _testHostFixture.ClearCookies();
        }

        [Fact]
        public async Task Logout_WhenLoggedInAndLogoutAllDevicesIsTrue_ShouldDeleteAllLoginRelatedToUser()
        {
            // Arrange
            var email = EmailGenerator.Generate();
            var password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var user = new User()
            {
                Email = email,
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(password)
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var token1 = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            var token2 = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            var login1 = new Login()
            {
                RefreshToken = token1.RefreshToken,
                ExpirationDate = token1.RefreshTokenExpirationDate,
                UserId = user.Id
            };
            var login2 = new Login()
            {
                RefreshToken = token2.RefreshToken,
                ExpirationDate = token2.RefreshTokenExpirationDate,
                UserId = user.Id
            };

            await _testHostFixture.AppDbContext.Logins.AddAsync(login1);
            await _testHostFixture.AppDbContext.Logins.AddAsync(login2);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            _testHostFixture.AppDbContext.UntrackEntity(login1);
            _testHostFixture.AppDbContext.UntrackEntity(login2);

            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });
            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.SetCookies(new Dictionary<string, string>() { { CookieUtility.RefreshTokenKey, token1.RefreshToken } });

            // Act
            var result = await _testHostFixture.Client.DeleteAsync(_logoutEndpoint + "?logoutAllDevices=true");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var logins = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.UserId.Equals(user.Id))
                .ToListAsync();
            Assert.Empty(logins);

            _testHostFixture.RemoveJwtBearerToken();
            _testHostFixture.ClearCookies();
        }
    }
}
