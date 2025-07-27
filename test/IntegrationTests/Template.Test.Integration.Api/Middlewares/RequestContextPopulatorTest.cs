using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Template.Api.Http;
using Template.Api.Utilities;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility;
using Template.Test.Utility.Fixtures.Hosting;

namespace Template.Test.Integration.Api.Middlewares
{
    [Collection(nameof(TestHostCollection))]
    public class RequestContextPopulatorTest
    {
        private readonly TestHostFixture _testHostFixture;

        private readonly Jwt _jwt;

        public RequestContextPopulatorTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;

            var config = ConfigurationHelper.CreateConfiguration();
            var jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;

            _jwt = new Jwt(Options.Create(jwtSettings), new TokenGenerator());
        }

        [Fact]
        public async Task RequestContextEndpoint_ShouldPopulateRequestContext()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            });

            _testHostFixture.SetCookies(new Dictionary<string, string>() { { CookieUtility.RefreshTokenKey, "refresh-token-value" } });
            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.UpdateUserAgent("TestAgent", "1.0");
            _testHostFixture.UpdateAcceptLanguage(new Dictionary<string, double>()
            {
                { "es-ES", 0.1 },
                { "fr-FR", 0.9 },
                { "de-DE", 0.5 },
                { "en", 1.0 }
            });

            // Act
            var response = await _testHostFixture.Client.GetAsync("/test/request-context");
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RequestContext>(content, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("refresh-token-value", result.RefreshToken);
            Assert.Equal("TestAgent/1.0", result.DeviceInfo);
            Assert.Equal("en", result.PreferredLanguages[0]);
            Assert.Equal("fr-FR", result.PreferredLanguages[1]);
            Assert.Equal("de-DE", result.PreferredLanguages[2]);
            Assert.Equal("es-ES", result.PreferredLanguages[3]);

            _testHostFixture.RemoveJwtBearerToken();
            _testHostFixture.ResetUserAgent();
            _testHostFixture.ResetAcceptLanguage();
        }
    }
}
