using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Template.Api.Utilities;
using Template.Application.Dtos.Controllers.Authentication;
using Template.Application.Features.Commands.Authentication.Login;
using Template.Application.Settings;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers
{
    public partial class AuthenticationControllerTest
    {
        private const string _loginEndpoint = "/auth/login";

        [Fact]
        public async Task Login_WhenUserDoesNotExists_ShouldReturnBadRequest()
        {
            // Arrange
            var request = new LoginRequest()
            {
                Email = EmailGenerator.Generate(),
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_loginEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task Login_AccountIsLocked_ShouldReturnLocked()
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
                    PasswordHashed = _hashing.HashPassword(password),
                    IsLocked = true,
                    UnlockDate = DateTime.UtcNow.AddDays(1)
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var request = new LoginRequest()
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_loginEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Locked, result.StatusCode);
        }

        [Fact]
        public async Task Login_WhenPasswordIsWrong_ShouldReturnBadRequestAndIncreaseFailedAttemps()
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

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            var request = new LoginRequest()
            {
                Email = email,
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_loginEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(1, securityState.FailedAttempts);
        }

        [Fact]
        public async Task Login_WhenPasswordIsWrongAndFailedAttempsReachesFirstLockCount_ShouldReturnLockedAndLockAccountAccordingly()
        {
            // Arrange
            var accountLockSettings = _testHostFixture.Configuration.GetSection(AccountLockSettings.SettingsKey).Get<AccountLockSettings>()!;

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
                    PasswordHashed = _hashing.HashPassword(password),
                    FailedAttempts = accountLockSettings.FailedAttemptsForFirstLock - 1,
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            var request = new LoginRequest()
            {
                Email = email,
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_loginEndpoint, request);

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
        public async Task Login_WhenPasswordIsWrongAndFailedAttempsReachesSecondLockCount_ShouldReturnLockedAndLockAccountAccordingly()
        {
            // Arrange
            var accountLockSettings = _testHostFixture.Configuration.GetSection(AccountLockSettings.SettingsKey).Get<AccountLockSettings>()!;

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
                    PasswordHashed = _hashing.HashPassword(password),
                    FailedAttempts = accountLockSettings.FailedAttemptsForSecondLock - 1,
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            var request = new LoginRequest()
            {
                Email = email,
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_loginEndpoint, request);

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
        public async Task Login_WhenCredentialsAreCorrect_ShouldReturnOkWithCookiesAndAccessToken()
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

            var request = new LoginRequest()
            {
                Email = email,
                Password = password
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_loginEndpoint, request);

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
                .Where(l => l.UserId.Equals(user.Id))
                .ToListAsync();
            Assert.Single(logins);
        }
    }
}
