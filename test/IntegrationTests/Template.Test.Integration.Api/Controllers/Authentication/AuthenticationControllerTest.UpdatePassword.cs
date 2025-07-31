using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Template.Application.Dtos.Crypto.Enums;
using Template.Application.Features.Commands.Authentication.UpdatePassword;
using Template.Application.Settings;
using Template.Domain.Entities.UserManagement;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.TestValues;

namespace Template.Test.Integration.Api.Controllers.Authentication
{
    public partial class AuthenticationControllerTest
    {
        private const string _updatePasswordEndpoint = "/auth/update-password";

        [Fact]
        public async Task UpdatePassword_WhenNotLoggedIn_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new UpdatePasswordRequest()
            {
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Theory]
        [MemberData(nameof(TestValues.GetInvalidPassword), MemberType = typeof(TestValues))]
        public async Task UpdatePassword_WhenNewPasswordIsInvalid_ShouldReturnBadRequest(string? newPassword)
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new UpdatePasswordRequest()
            {
                NewPassword = newPassword,
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Theory]
        [MemberData(nameof(TestValues.GetInvalidPassword), MemberType = typeof(TestValues))]
        public async Task UpdatePassword_WhenPasswordIsInvalid_ShouldReturnBadRequest(string? password)
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new UpdatePasswordRequest()
            {
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
                Password = password
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_WhenUserDoesNotExist_ShouldReturnInternalServerError()
        {
            // Arrange
            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var request = new UpdatePasswordRequest()
            {
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task UpdatePassword_WhenPasswordIsWrong_ShouldReturnBadRequestAndIncreaseFailedAttemps()
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

            var request = new UpdatePasswordRequest()
            {
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
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
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(1, securityState.FailedAttempts);
        }

        [Fact]
        public async Task UpdatePassword_WhenPasswordIsWrongAndFailedAttempsReachesFirstLockCount_ShouldReturnLockedAndLockAccountAccordingly()
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

            var request = new UpdatePasswordRequest()
            {
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
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
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

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
        public async Task UpdatePassword_WhenPasswordIsWrongAndFailedAttempsReachesSecondLockCount_ShouldReturnLockedAndLockAccountAccordingly()
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

            var request = new UpdatePasswordRequest()
            {
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
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
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

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
        public async Task UpdatePassword_WhenPasswordIsCorrect_ShouldReturnOkAndUpdatePasswordAndResetFailedAttemps()
        {
            // Arrange
            var password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var user = new User()
            {
                Email = EmailGenerator.Generate(),
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(password),
                    FailedAttempts = 1
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var jwtToken = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            });

            _testHostFixture.AddJwtBearerToken(jwtToken.AccessToken);

            var newPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var request = new UpdatePasswordRequest()
            {
                NewPassword = newPassword,
                Password = password
            };

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_updatePasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(EPasswordVerificationResult.Success, _hashing.VerifyPassword(securityState.PasswordHashed!, newPassword));
            Assert.Equal(0, securityState.FailedAttempts);
        }
    }
}
