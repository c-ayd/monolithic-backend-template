using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using Template.Application.Features.Commands.Authentication.VerifyEmail;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Entities.UserManagement.Enums;
using Template.Test.Utility.Extensions.EFCore;

namespace Template.Test.Integration.Api.Controllers
{
    public partial class AuthenticationControllerTest
    {
        private const string _verifyEmailEndpoint = "/auth/verify-email";

        [Fact]
        public async Task VerifyEmail_WhenTokenIsNotFound_ShouldReturnNotFound()
        {
            // Arrange
            var request = new VerifyEmailRequest()
            {
                Token = StringGenerator.GenerateUsingAsciiChars(10)
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_verifyEmailEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task VerifyEmail_WhenTokenIsExpired_ShouldReturnGone()
        {
            // Arrange
            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var user = new User()
            {
                Tokens = new List<Token>()
                {
                    new Token()
                    {
                        Value = _hashing.HashSha256(tokenValue),
                        Purpose = ETokenPurpose.EmailVerification,
                        ExpirationDate = DateTime.UtcNow.AddDays(-1)
                    }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var request = new VerifyEmailRequest()
            {
                Token = tokenValue
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_verifyEmailEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Gone, result.StatusCode);
        }

        [Fact]
        public async Task VerifyEmail_WhenSecurityStateOfUserNotFound_ShouldReturnInternalServerError()
        {
            // Arrange
            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var user = new User()
            {
                Tokens = new List<Token>()
                {
                    new Token()
                    {
                        Value = _hashing.HashSha256(tokenValue),
                        Purpose = ETokenPurpose.EmailVerification,
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            var isDeleted = typeof(User).GetProperty(nameof(User.IsDeleted), BindingFlags.Instance | BindingFlags.Public)!.GetSetMethod(nonPublic: true)!;
            isDeleted.Invoke(user, new object[] { true });

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var request = new VerifyEmailRequest()
            {
                Token = tokenValue
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_verifyEmailEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
        }

        [Fact]
        public async Task VerifyEmail_WhenTokenIsValid_ShouldReturnOkAndVerifyEmailOfUser()
        {
            // Arrange
            var tokenValue = StringGenerator.GenerateUsingAsciiChars(10);
            var user = new User()
            {
                SecurityState = new SecurityState()
                {
                    IsEmailVerified = false
                },
                Tokens = new List<Token>()
                {
                    new Token()
                    {
                        Value = _hashing.HashSha256(tokenValue),
                        Purpose = ETokenPurpose.EmailVerification,
                        ExpirationDate = DateTime.UtcNow.AddDays(1)
                    }
                }
            };

            await _testHostFixture.AppDbContext.Users.AddAsync(user);
            await _testHostFixture.AppDbContext.SaveChangesAsync();

            var userId = user.Id;
            _testHostFixture.AppDbContext.UntrackEntity(user.SecurityState);
            _testHostFixture.AppDbContext.UntrackEntity(user);

            var request = new VerifyEmailRequest()
            {
                Token = tokenValue
            };

            // Act
            var result = await _testHostFixture.Client.PatchAsJsonAsync(_verifyEmailEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var securityState = await _testHostFixture.AppDbContext.SecurityStates
                .Where(ss => ss.UserId.Equals(userId))
                .FirstOrDefaultAsync();
            Assert.NotNull(securityState);
            Assert.True(securityState.IsEmailVerified, "The email is not verified.");
        }
    }
}
