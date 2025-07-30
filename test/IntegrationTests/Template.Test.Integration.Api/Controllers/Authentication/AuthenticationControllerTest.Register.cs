using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using Template.Application.Features.Commands.Authentication.Register;
using Template.Domain.Entities.UserManagement.Enums;
using Template.Test.Utility;
using Template.Test.Utility.TestValues;

namespace Template.Test.Integration.Api.Controllers.Authentication
{
    public partial class AuthenticationControllerTest
    {
        private const string _registerEndpoint = "/auth/register";

        [Theory]
        [MemberData(nameof(TestValues.GetInvalidEmails), MemberType = typeof(TestValues))]
        public async Task Register_WhenEmailAddressIsInvalid_ShouldReturnBadRequest(string? email)
        {
            // Arrange
            var request = new RegisterRequest()
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
            var result = await _testHostFixture.Client.PostAsJsonAsync(_registerEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Theory]
        [MemberData(nameof(TestValues.GetInvalidPassword), MemberType = typeof(TestValues))]
        public async Task Register_WhenPasswordIsInvalid_ShouldReturnBadRequest(string? password)
        {
            // Arrange
            var request = new RegisterRequest()
            {
                Email = EmailGenerator.Generate(),
                Password = password
            };

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_registerEndpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

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
        public async Task Register_WhenUserIsCreatedButEmailIsNotSend_ShouldReturnMultiStatusAndCreateUserAndCreateEmailVerificationToken()
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
        }

        [Fact]
        public async Task Register_WhenUserIsCreatedAndEmailIsSent_ShouldReturnOkAndCreateUserAndCreateEmailVerificationTokenAndSendEmail()
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

            var email = await EmailHelper.GetLatestTempEmailFileAsync();
            Assert.NotNull(email);
            Assert.Equal(request.Email, email.ReceiverEmail);
        }
    }
}
