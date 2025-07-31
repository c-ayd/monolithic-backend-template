using Microsoft.Extensions.Options;
using System.Net;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;

namespace Template.Test.Integration.Api
{
    public partial class AuthTest
    {
        private const string _authenticationEndpoint = "/test/authentication";

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
    }
}
