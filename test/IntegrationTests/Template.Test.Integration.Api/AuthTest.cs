using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Integration.Api.Collections;
using Template.Test.Utility;
using Template.Test.Utility.Fixtures.Hosting;
using Template.Test.Utility.Hosting.Policies;

namespace Template.Test.Integration.Api
{
    [Collection(nameof(TestHostCollection))]
    public class AuthTest
    {
        private readonly TestHostFixture _testHostFixture;
        private readonly JwtSettings _jwtSettings;
        private readonly Jwt _jwt;

        private readonly string _authenticationEndpoint = "/test/authentication";
        private readonly string _authorizationEndpoint = "/test/authorization";

        public AuthTest(TestHostFixture testHostFixture)
        {
            _testHostFixture = testHostFixture;

            var config = ConfigurationHelper.CreateConfiguration();
            _jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;

            _jwt = new Jwt(Options.Create(_jwtSettings), new TokenGenerator());
        }

        // ~ Begin - Authentication tests
        [Fact]
        public async Task AuthenticationEndpoint_WhenJwtBearerTokenExistsAndIsValid_ShouldReturnOk()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken();
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authenticationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthenticationEndpoint_WhenJwtBearerTokenExistsButInvalid_ShouldReturnUnauthorized()
        {
            // Arrange
            _jwtSettings.Issuer = "test-issuer";
            _jwtSettings.Audience = "test-audience";
            var jwt = new Jwt(Options.Create(_jwtSettings), new TokenGenerator());

            var tokens = jwt.GenerateJwtToken();
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authenticationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthenticationEndpoint_WhenJwtBearerTokenExistsButExpired_ShouldReturnUnauthorized()
        {
            // Arrange
            _jwtSettings.AccessTokenLifeSpanInMinutes = -1;
            var jwt = new Jwt(Options.Create(_jwtSettings), new TokenGenerator());

            var tokens = jwt.GenerateJwtToken();
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authenticationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthenticationEndpoint_WhenJwtBearerTokenExistsButNotActiveYet_ShouldReturnUnauthorized()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken(DateTime.UtcNow.AddMinutes(1));
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authenticationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthenticationEndpoint_WhenJwtBearerTokenDoesNotExist_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _testHostFixture.Client.GetAsync(_authenticationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        //~ End

        //~ Begin - Authorization/Policy tests
        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenAndClaimExistAndAreValid_ShouldReturnOk()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(TestPolicy.ClaimName, "test-value")
            });
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenAndClaimExistButClaimValueIsWrong_ShouldReturnForbidden()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(TestPolicy.ClaimName, "wrong-value")
            });
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenExistsButClaimDoesNotExist_ShouldReturnForbidden()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken();
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenExistsButInvalid_ShouldReturnUnauthorized()
        {
            // Arrange
            _jwtSettings.Issuer = "test-issuer";
            _jwtSettings.Audience = "test-audience";
            var jwt = new Jwt(Options.Create(_jwtSettings), new TokenGenerator());

            var tokens = jwt.GenerateJwtToken();
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenExistsButExpired_ShouldReturnUnauthorized()
        {
            // Arrange
            _jwtSettings.AccessTokenLifeSpanInMinutes = -1;
            var jwt = new Jwt(Options.Create(_jwtSettings), new TokenGenerator());

            var tokens = jwt.GenerateJwtToken();
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenExistsButNotActiveYet_ShouldReturnUnauthorized()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken(DateTime.UtcNow.AddMinutes(1));
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenDoesNotExist_ShouldReturnUnauthorized()
        {
            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        //~ End
    }
}
