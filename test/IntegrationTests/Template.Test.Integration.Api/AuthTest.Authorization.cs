using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Test.Utility.Hosting.Policies;

namespace Template.Test.Integration.Api
{
    public partial class AuthTest
    {
        private const string _authorizationEndpoint = "/test/authorization";

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenAndClaimExistAndAreValid_ShouldReturnOk()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.Role, TestPolicy.RoleName)
            });
            _testHostFixture.AddJwtBearerToken(tokens.AccessToken);

            // Act
            var response = await _testHostFixture.Client.GetAsync(_authorizationEndpoint);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _testHostFixture.RemoveJwtBearerToken();
        }

        [Fact]
        public async Task AuthorizationEndpoint_WhenJwtBearerTokenAndMoreClaimsThanNeededExistAndAreValid_ShouldReturnOk()
        {
            // Arrange
            var tokens = _jwt.GenerateJwtToken(new List<Claim>()
            {
                new Claim(ClaimTypes.Role, TestPolicy.RoleName),
                new Claim(ClaimTypes.Role, "test-value-2"),
                new Claim(ClaimTypes.Role, "test-value-3")
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
                new Claim(ClaimTypes.Role, "wrong-value")
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
    }
}
