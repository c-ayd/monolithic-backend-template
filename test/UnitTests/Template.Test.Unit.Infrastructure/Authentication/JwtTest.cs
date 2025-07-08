using Cayd.Test.Generators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Utility;

namespace Template.Test.Unit.Infrastructure.Authentication
{
    public class JwtTest
    {
        private readonly Jwt _jwt;

        public JwtTest()
        {
            var config = ConfigurationHelper.CreateConfiguration();
            var jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;

            _jwt = new Jwt(Options.Create(jwtSettings), new TokenGenerator());
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GenerateJwtToken_WhenNoClaimsIsGiven_ShouldGenerateToken(bool addClaims)
        {
            // Arrange
            List<Claim>? claims = null;
            if (addClaims)
            {
                claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, StringGenerator.GenerateUsingAsciiChars(10)),
                    new Claim(ClaimTypes.Email, EmailGenerator.Generate())
                };
            }

            // Act
            var result = _jwt.GenerateJwtToken(claims);

            // Assert
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.NotNull(result.RefreshTokenExpirationDate);
        }
    }
}
