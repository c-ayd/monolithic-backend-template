using Microsoft.Extensions.Options;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility.Fixtures.Hosting;
using Template.Test.Utility;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Security.Claims;
using Template.Application.Policies;

namespace Template.Test.Integration.Api.Policies
{
    [Collection(nameof(TestHostCollection))]
    public class EmailVerificationPolicyTest
    {
        public const string endpoint = "/test/email-verification-policy";

        private readonly TestHostFixture _testHostFixture;
        private readonly Jwt _jwt;

        public EmailVerificationPolicyTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;

            var config = ConfigurationHelper.CreateConfiguration();
            var jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;
            _jwt = new Jwt(Options.Create(jwtSettings), new TokenGenerator());
        }

        [Fact]
        public async Task EmailVerificationPolicyEndpoint_WhenClaimIsMissing_ShouldReturnForbidden()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task EmailVerificationPolicyEndpoint_WhenClaimValueIsWrong_ShouldReturnForbidden()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(EmailVerificationPolicy.ClaimName, false.ToString())
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task EmailVerificationPolicyEndpoint_WhenClaimValueIsCorrect_ShouldReturnOk()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(EmailVerificationPolicy.ClaimName, true.ToString())
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
