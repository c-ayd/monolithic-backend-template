using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using Template.Application.Policies;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Policies
{
    [Collection(nameof(TestHostCollection))]
    public class AdminPolicyTest
    {
        public const string endpoint = "/test/admin-policy";

        private readonly TestHostFixture _testHostFixture;
        private readonly Jwt _jwt;

        public AdminPolicyTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;

            var config = ConfigurationHelper.CreateConfiguration();
            var jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;
            _jwt = new Jwt(Options.Create(jwtSettings), new TokenGenerator());
        }

        [Fact]
        public async Task AdminPolicyEndpoint_WhenClaimIsMissing_ShouldReturnForbidden()
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
        public async Task AdminPolicyEndpoint_WhenClaimValueIsWrong_ShouldReturnForbidden()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, "test-value")
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task AdminPolicyEndpoint_WhenClaimValueIsCorrect_ShouldReturnOk()
        {
            // Arrange
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, AdminPolicy.RoleName)
            });

            _testHostFixture.AddJwtBearerToken(token.AccessToken);

            // Act
            var result = await _testHostFixture.Client.GetAsync(endpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
