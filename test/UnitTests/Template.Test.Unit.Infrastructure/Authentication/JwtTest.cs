using Cayd.Test.Generators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Settings.Authentication;
using Template.Test.Utility;

namespace Template.Test.Unit.Infrastructure.Authentication
{
    public class JwtTest
    {
        private readonly Jwt _jwt;
        private readonly JwtSettings _jwtSettings;

        public JwtTest()
        {
            var config = ConfigurationHelper.CreateConfiguration();
            _jwtSettings = config.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;

            _jwt = new Jwt(Options.Create(_jwtSettings), new TokenGenerator());
        }

        private (List<Claim>?, DateTime?, DateTime?) DecodeAccessToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,

                ValidAudience = _jwtSettings.Audience,
                ValidIssuer = _jwtSettings.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
            };

            try
            {
                var claimsPrincipal = handler.ValidateToken(accessToken, validationParams, out var token);
                if (token is not JwtSecurityToken jwtToken)
                {
                    Assert.Fail("Validated token is not a JWT token.");
                    return (null, null, null);
                }

                return (claimsPrincipal.Claims.ToList(), jwtToken.ValidFrom, jwtToken.ValidTo);
            }
            catch (Exception exception)
            {
                Assert.Fail($"Validation Failed: {exception.Message}");
                return (null, null, null);
            }
        }

        [Fact]
        public void GenerateJwtToken_WhenClaimsAndNotBeforeDateTimeAreNotGiven_ShouldGenerateToken()
        {
            // Act
            var result = _jwt.GenerateJwtToken();

            // Assert
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);

            var totalMinutes = (result.RefreshTokenExpirationDate - DateTime.UtcNow).TotalMinutes;
            var addedMinutes = _jwtSettings.RefreshTokenLifeSpanInDays * 24 * 60;
            Assert.True(totalMinutes >= addedMinutes - 1 && totalMinutes <= addedMinutes, $"The expiration date is not in range. Total minutes: {totalMinutes}");
        }

        [Fact]
        public void GenerateJwtToken_WhenClaimsAreNotGivenButNotBeforeDateTimeIsGiven_ShouldGenerateToken()
        {
            // Arrange
            var notBefore = DateTime.UtcNow.AddDays(1);

            // Act
            var result = _jwt.GenerateJwtToken(notBefore);

            // Assert
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);

            var totalMinutes = (result.RefreshTokenExpirationDate - notBefore).TotalMinutes;
            var addedMinutes = _jwtSettings.RefreshTokenLifeSpanInDays * 24 * 60;
            Assert.True(totalMinutes >= addedMinutes - 1 && totalMinutes <= addedMinutes, $"The expiration date is not in range. Total minutes: {totalMinutes}");

            var (_, decodedNotBefore, _) = DecodeAccessToken(result.AccessToken);
            Assert.Equal(notBefore.ToString("dd-MM-yyyy HH:mm:ss"), decodedNotBefore!.Value.ToString("dd-MM-yyyy HH:mm:ss"));
        }

        [Fact]
        public void GenerateJwtToken_WhenClaimsAndNotBeforeDateTimeAreGiven_ShouldGenerateToken()
        {
            // Arrange
            var nameIdentifier = StringGenerator.GenerateUsingAsciiChars(10);
            var email = EmailGenerator.Generate();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                new Claim(ClaimTypes.Email, email)
            };

            var notBefore = DateTime.UtcNow.AddDays(1);

            // Act
            var result = _jwt.GenerateJwtToken(claims, notBefore);

            // Assert
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);

            var totalMinutes = (result.RefreshTokenExpirationDate - notBefore).TotalMinutes;
            var addedMinutes = _jwtSettings.RefreshTokenLifeSpanInDays * 24 * 60;
            Assert.True(totalMinutes >= addedMinutes - 1 && totalMinutes <= addedMinutes, $"The expiration date is not in range. Total minutes: {totalMinutes}");

            var (decodedClaims, _, _) = DecodeAccessToken(result.AccessToken);
            var decodedNameIdentifier = decodedClaims!.Find(c => c.Type == ClaimTypes.NameIdentifier)!.Value;
            var decodedEmail = decodedClaims!.Find(c => c.Type == ClaimTypes.Email)!.Value;
            Assert.Equal(nameIdentifier, decodedNameIdentifier);
            Assert.Equal(email, decodedEmail);
        }

        [Fact]
        public void GenerateJwtToken_WhenClaimsAreGivenButNotBeforeDateTimeIsNotGiven_ShouldGenerateToken()
        {
            // Arrange
            List<Claim>? claims = claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, StringGenerator.GenerateUsingAsciiChars(10)),
                new Claim(ClaimTypes.Email, EmailGenerator.Generate())
            };

            // Act
            var result = _jwt.GenerateJwtToken(claims);

            // Assert
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);

            var totalMinutes = (result.RefreshTokenExpirationDate - DateTime.UtcNow).TotalMinutes;
            var addedMinutes = _jwtSettings.RefreshTokenLifeSpanInDays * 24 * 60;
            Assert.True(totalMinutes >= addedMinutes - 1 && totalMinutes <= addedMinutes, $"The expiration date is not in range. Total minutes: {totalMinutes}");
        }
    }
}
