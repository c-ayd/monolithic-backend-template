using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using Template.Application.Features.Commands.Authentication.Register;
using Template.Domain.Entities.UserManagement.Enums;
using Template.Test.Utility;

namespace Template.Test.Integration.Api.Controllers
{
    public partial class AuthenticationControllerTest
    {
        private const string _registerEndpoint = "/auth/register";

        [Fact]
        public async Task Register_WhenUserExists_ShouldReturnConflict()
        {
            // Arrange
            var request = new RegisterRequest()
            {
                Email = EmailGenerator.Generate(),
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            await _testHostFixture.Client.PostAsJsonAsync(_registerEndpoint, request);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_registerEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Register_WhenUserIsCreatedButEmailIsNotSend_ShouldReturnMultiStatusAndCreateUserAndToken()
        {
            // Arrange
            var request = new RegisterRequest()
            {
                Email = EmailGenerator.Generate(),
                Password = PasswordGenerator.GenerateWithCustomRules(
                    length: 10,
                    requireDigit: true,
                    requireLowercase: false,
                    requireUppercase: false,
                    requireNonAlphanumeric: false)
            };

            EmailHelper.SetEmailSenderResult(false);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_registerEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.MultiStatus, result.StatusCode);

            var user = await _testHostFixture.AppDbContext.Users
                .Where(u => u.Email == request.Email)
                .FirstOrDefaultAsync();
            Assert.NotNull(user);

            var token = await _testHostFixture.AppDbContext.Tokens
                .Where(t => t.UserId.Equals(user.Id))
                .FirstOrDefaultAsync();
            Assert.NotNull(token);
            Assert.Equal(ETokenPurpose.EmailVerification, token.Purpose);

            EmailHelper.SetEmailSenderResult(true);
        }

        [Fact]
        public async Task Register_WhenUserIsCreatedAndEmailIsSent_ShouldReturnOkAndCreateUserAndGenerateTokenAndSendEmail()
        {
            // Arrange
            var request = new RegisterRequest()
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
            var result = await _testHostFixture.Client.PostAsJsonAsync(_registerEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var user = await _testHostFixture.AppDbContext.Users
                .Where(u => u.Email == request.Email)
                .FirstOrDefaultAsync();
            Assert.NotNull(user);

            var token = await _testHostFixture.AppDbContext.Tokens
                .Where(t => t.UserId.Equals(user.Id))
                .FirstOrDefaultAsync();
            Assert.NotNull(token);
            Assert.Equal(ETokenPurpose.EmailVerification, token.Purpose);

            var email = await EmailHelper.GetLatestTempEmailFile();
            Assert.NotNull(email);
            Assert.Equal(request.Email, email.ReceiverEmail);
        }
    }
}
