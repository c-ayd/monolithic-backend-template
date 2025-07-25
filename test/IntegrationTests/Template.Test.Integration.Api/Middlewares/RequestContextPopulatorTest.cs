using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Template.Api.Http;
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

            _testHostFixture.AddJwtBearerToken(token.AccessToken);
            _testHostFixture.UpdateUserAgent("TestAgent", "1.0");
            _testHostFixture.UpdateAcceptLanguage("es-ES");

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
            Assert.Equal("TestAgent/1.0", result.DeviceInfo);
            Assert.Single(result.PreferredLanguages);
            Assert.Equal("es-ES", result.PreferredLanguages[0]);

            _testHostFixture.RemoveJwtBearerToken();
            _testHostFixture.ResetUserAgent();
            _testHostFixture.ResetAcceptLanguage();
        }
    }
}
