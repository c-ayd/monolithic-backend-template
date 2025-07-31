using System.Net.Http.Json;
using System.Net;
using System.Security.Claims;
using Template.Test.Utility.TestValues;
using Template.Application.Features.Queries.Authentication.GetLogins;
using Cayd.Test.Generators;
using Template.Application.Settings;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Template.Api.Utilities;
using Template.Application.Dtos.Entities.UserManagement;

namespace Template.Test.Integration.Api.Controllers.Authentication
{
    public partial class AuthenticationControllerTest
    {
        public const string _getLoginsEndpoint = "/auth/logins";

        [Fact]
        public async Task GetLogins_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new GetLoginsRequest()
            {
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_getLoginsEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [MemberData(nameof(TestValues.GetInvalidPassword), MemberType = typeof(TestValues))]
        public async Task GetLogins_WhenPasswordIsInvalid_ShouldReturnBadRequest(string? password)
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new GetLoginsRequest()
            {
                Password = password
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_getLoginsEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task GetLogins_WhenUserDoesNotExist_ShouldReturnInternalServerError()
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new GetLoginsRequest()
            {
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_getLoginsEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task GetLogins_WhenPasswordIsWrong_ShouldReturnBadRequestAndIncreaseFailedAttemps()
        {
            // Arrange
            var user = new User()
            {
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(StringGenerator.GenerateUsingAsciiChars(15))
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new GetLoginsRequest()
            {
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_getLoginsEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(1, securityState.FailedAttempts);
        }

        [Fact]
        public async Task GetLogins_WhenPasswordIsWrongAndFailedAttempsReachesFirstLockCount_ShouldReturnLockedAndLockAccountAccordingly()
        {
            // Arrange
            var accountLockSettings = _testHostFixture.Configuration.GetSection(AccountLockSettings.SettingsKey).Get<AccountLockSettings>()!;

            var user = new User()
            {
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(StringGenerator.GenerateUsingAsciiChars(15)),
                    FailedAttempts = accountLockSettings.FailedAttemptsForFirstLock - 1,
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new GetLoginsRequest()
            {
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_getLoginsEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Locked, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(accountLockSettings.FailedAttemptsForFirstLock, securityState.FailedAttempts);
            Assert.True(securityState.IsLocked, "The account is not marked as locked.");
            Assert.NotNull(securityState.UnlockDate);

            var totalMinutes = (securityState.UnlockDate.Value - DateTime.UtcNow).TotalMinutes;
            Assert.True(totalMinutes >= accountLockSettings.FirstLockTimeInMinutes - 1 && totalMinutes <= accountLockSettings.FirstLockTimeInMinutes,
                $"The unlock date is not in range. Total minutes: {totalMinutes}");
        }

        [Fact]
        public async Task GetLogins_WhenPasswordIsWrongAndFailedAttempsReachesSecondLockCount_ShouldReturnLockedAndLockAccountAccordingly()
        {
            // Arrange
            var accountLockSettings = _testHostFixture.Configuration.GetSection(AccountLockSettings.SettingsKey).Get<AccountLockSettings>()!;

            var user = new User()
            {
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(StringGenerator.GenerateUsingAsciiChars(15)),
                    FailedAttempts = accountLockSettings.FailedAttemptsForSecondLock - 1,
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new GetLoginsRequest()
            {
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_getLoginsEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Locked, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(0, securityState.FailedAttempts);
            Assert.True(securityState.IsLocked, "The account is not marked as locked.");
            Assert.NotNull(securityState.UnlockDate);

            var totalMinutes = (securityState.UnlockDate.Value - DateTime.UtcNow).TotalMinutes;
            Assert.True(totalMinutes >= accountLockSettings.SecondLockTimeInMinutes - 1 && totalMinutes <= accountLockSettings.SecondLockTimeInMinutes,
                $"The unlock date is not in range. Total minutes: {totalMinutes}");
        }

        [Fact]
        public async Task GetLogins_WhenPasswordIsCorrect_ShouldReturnLoginsAndResetFailedAttemps()
        {
            var password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var user = new User()
            {
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(password),
                    FailedAttempts = 1,
                },
                Logins = new List<Login>()
                {
                    new Login()
                    {
                        RefreshTokenHashed = StringGenerator.GenerateUsingAsciiChars(10),
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    },
                    new Login()
                    {
                        RefreshTokenHashed = StringGenerator.GenerateUsingAsciiChars(10),
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    },
                    new Login()
                    {
                        RefreshTokenHashed = StringGenerator.GenerateUsingAsciiChars(10),
                        ExpirationDate = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new GetLoginsRequest()
            {
                Password = password
            };

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_getLoginsEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var json = await result.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(json);
            var dataElement = jsonDocument.RootElement.GetProperty(JsonUtility.DataKey);
            var responseData = JsonSerializer.Deserialize<List<LoginDto>>(dataElement.GetRawText(), new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(responseData);
            Assert.Equal(2, responseData.Count);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.False(securityState.IsEmailVerified, "The new email is in the verified state.");
            Assert.Equal(0, securityState.FailedAttempts);
        }
    }
}
