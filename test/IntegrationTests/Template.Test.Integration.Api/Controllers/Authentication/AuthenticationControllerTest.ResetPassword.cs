using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Template.Application.Dtos.Crypto.Enums;
using Template.Application.Features.Commands.Authentication.ResetPassword;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Entities.UserManagement.Enums;
using Template.Test.Utility.Extensions.EFCore;
using Template.Test.Utility.TestValues;

namespace Template.Test.Integration.Api.Controllers.Authentication
{
    public partial class AuthenticationControllerTest
    {
        private const string _resetPasswordEndpoint = "/auth/reset-password";

        [Theory]
        [MemberData(nameof(TestValues.GetInvalidStrings), MemberType = typeof(TestValues))]
        public async Task ResetPassword_WhenTokenIsInvalid_ShouldReturnBadRequest(string? token)
        {
            // Arrange
            var request = new ResetPasswordRequest()
            {
                Token = token,
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
                LogoutAllDevices = false
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_resetPasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Theory]
        [MemberData(nameof(TestValues.GetInvalidEmails), MemberType = typeof(TestValues))]
        public async Task ResetPassword_WhenNewPasswordIsInvalid_ShouldReturnBadRequest(string? password)
        {
            // Arrange
            var request = new ResetPasswordRequest()
            {
                Token = StringGenerator.GenerateUsingAsciiChars(10),
                NewPassword = password,
                LogoutAllDevices = false
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_resetPasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_WhenTokenIsNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var request = new ResetPasswordRequest()
            {
                Token = StringGenerator.GenerateUsingAsciiChars(10),
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
                LogoutAllDevices = false
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_resetPasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_WhenTokenIsExpired_ShouldReturnGone()
        {
            // Arrange
            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var user = new User()
            {
                Tokens = new List<Token>()
                {
                    new Token()
                    {
                        ValueHashed = _hashing.HashSha256(tokenValue),
                        Purpose = ETokenPurpose.ResetPassword,
                        ExpirationDate = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var request = new ResetPasswordRequest()
            {
                Token = tokenValue,
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
                LogoutAllDevices = false
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_resetPasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Gone, result.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_WhenSecurityStateOfUserNotFound_ShouldReturnInternalServerError()
        {
            // Arrange
            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var user = new User()
            {
                Tokens = new List<Token>()
                {
                    new Token()
                    {
                        ValueHashed = _hashing.HashSha256(tokenValue),
                        Purpose = ETokenPurpose.ResetPassword,
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var isDeleted = typeof(User).GetProperty(nameof(User.IsDeleted), BindingFlags.Instance | BindingFlags.Public)!.GetSetMethod(nonPublic: true)!;
            isDeleted.Invoke(user, new object[] { true });

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var request = new ResetPasswordRequest()
            {
                Token = tokenValue,
                NewPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false),
                LogoutAllDevices = false
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_resetPasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task ResetPassword_WhenTokenIsValidAndLogoutAllDevicesIsFalse_ShouldReturnOkAndUpdatePasswordAndKeepLoginsAndDeleteAllTokensExceptNewEmailVerificationToken()
        {
            // Arrange
            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var user = new User()
            {
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(PasswordGenerator.Generate())
                },
                Logins = new List<Login>()
                {
                    new Login()
                    {
                        RefreshTokenHashed = _hashing.HashSha256(StringGenerator.GenerateUsingAsciiChars(10)),
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    }
                },
                Tokens = new List<Token>()
                {
                    new Token()
                    {
                        ValueHashed = _hashing.HashSha256(tokenValue),
                        Purpose = ETokenPurpose.ResetPassword,
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    },
                    new Token()
                    {
                        ValueHashed = _hashing.HashSha256(StringGenerator.GenerateUsingAsciiChars(10)),
                        Purpose = ETokenPurpose.EmailVerification,
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntities(user.Tokens.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            var newPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var request = new ResetPasswordRequest()
            {
                Token = tokenValue,
                NewPassword = newPassword,
                LogoutAllDevices = false
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_resetPasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(EPasswordVerificationResult.Success, _hashing.VerifyPassword(securityState.PasswordHashed!, newPassword));

            var logins = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.UserId.Equals(userId))
                .ToListAsync();
            Assert.Single(logins);

            var tokens = await _testHostFixture.AppDbContext.Tokens
                .Where(t => t.UserId.Equals(userId))
                .ToListAsync();
            Assert.Single(tokens);
            Assert.Equal(ETokenPurpose.EmailVerification, tokens[0].Purpose);
        }

        [Fact]
        public async Task ResetPassword_WhenTokenIsValidAndLogoutAllDevicesIsTrue_ShouldReturnOkAndUpdatePasswordAndDeleteAllLoginsAndDeleteAllTokensExceptNewEmailVerificationToken()
        {
            // Arrange
            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var user = new User()
            {
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(PasswordGenerator.Generate())
                },
                Logins = new List<Login>()
                {
                    new Login()
                    {
                        RefreshTokenHashed = _hashing.HashSha256(StringGenerator.GenerateUsingAsciiChars(10)),
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    }
                },
                Tokens = new List<Token>()
                {
                    new Token()
                    {
                        ValueHashed = _hashing.HashSha256(tokenValue),
                        Purpose = ETokenPurpose.ResetPassword,
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    },
                    new Token()
                    {
                        ValueHashed = _hashing.HashSha256(StringGenerator.GenerateUsingAsciiChars(10)),
                        Purpose = ETokenPurpose.EmailVerification,
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntities(user.Logins.ToArray());
            _testHostFixture.AppDbContext.UntrackEntities(user.Tokens.ToArray());
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            var newPassword = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false);

            var request = new ResetPasswordRequest()
            {
                Token = tokenValue,
                NewPassword = newPassword,
                LogoutAllDevices = true
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_resetPasswordEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.Equal(EPasswordVerificationResult.Success, _hashing.VerifyPassword(securityState.PasswordHashed!, newPassword));

            var logins = await _testHostFixture.AppDbContext.Logins
                .Where(l => l.UserId.Equals(userId))
                .ToListAsync();
            Assert.Empty(logins);

            var tokens = await _testHostFixture.AppDbContext.Tokens
                .Where(t => t.UserId.Equals(userId))
                .ToListAsync();
            Assert.Single(tokens);
            Assert.Equal(ETokenPurpose.EmailVerification, tokens[0].Purpose);
        }
    }
}
