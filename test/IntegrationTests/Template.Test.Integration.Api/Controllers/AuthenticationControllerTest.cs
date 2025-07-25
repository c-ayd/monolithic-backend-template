using Cayd.Test.Generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Template.Application.Features.Commands.Authentication.Register;
using Template.Persistence.DbContexts;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Controllers
{
    [Collection(nameof(TestHostCollection))]
    public class AuthenticationControllerTest
    {
        private readonly TestHostFixture _testHostFixture;
        private readonly string _endpoint = "/auth/register";

        public AuthenticationControllerTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;
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

            await _testHostFixture.Client.PostAsJsonAsync(_endpoint, request);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_endpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        }

        [Fact]
        public async Task Register_WhenUserIsCreatedButEmailIsNotSend_ShouldReturnMultiStatus()
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

            AppDomain.CurrentDomain.SetData("EmailSenderResult", false);

            // Act
            var result = await _testHostFixture.Client.PostAsJsonAsync(_endpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.MultiStatus, result.StatusCode);

            AppDomain.CurrentDomain.SetData("EmailSenderResult", true);
        }

        [Fact]
        public async Task Register_WhenUserIsCreatedAndEmailIsSent_ShouldReturnOk()
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
            var result = await _testHostFixture.Client.PostAsJsonAsync(_endpoint, request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            var dbContext = _testHostFixture.Host.Services.GetRequiredService<AppDbContext>();
            var user = await dbContext.Users
                .Where(u => u.Email == request.Email)
                .FirstOrDefaultAsync();
            Assert.NotNull(user);
        }
    }
}
