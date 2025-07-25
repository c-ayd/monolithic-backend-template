using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Template.Application.Abstractions.Authentication;
using Template.Application.Abstractions.Crypto;
using Template.Application.Dtos.Authentication;
using Template.Infrastructure.Settings.Authentication;

namespace Template.Infrastructure.Authentication
{
    public class Jwt : IJwt
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ITokenGenerator _tokenGenerator;

        public Jwt(IOptions<JwtSettings> jwtSettings,
            ITokenGenerator tokenGenerator)
        {
            _jwtSettings = jwtSettings.Value;
            _tokenGenerator = tokenGenerator;
        }

        public JwtTokenDto GenerateJwtToken()
            => Generate(null, null);

        public JwtTokenDto GenerateJwtToken(ICollection<Claim> claims)
            => Generate(claims, null);

        public JwtTokenDto GenerateJwtToken(ICollection<Claim> claims, DateTime notBefore)
            => Generate(claims, notBefore);

        public JwtTokenDto GenerateJwtToken(DateTime notBefore)
            => Generate(null, notBefore);

        public JwtTokenDto Generate(ICollection<Claim>? claims, DateTime? notBefore)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var token = new JwtSecurityToken(
                claims: claims,
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                notBefore: notBefore,
                expires: notBefore != null ?
                    notBefore.Value.AddMinutes(_jwtSettings.AccessTokenLifeSpanInMinutes) :
                    DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenLifeSpanInMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtTokenDto()
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = _tokenGenerator.Generate(),
                RefreshTokenExpirationDate = notBefore != null ?
                    notBefore.Value.AddDays(_jwtSettings.RefreshTokenLifeSpanInDays) :
                    DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenLifeSpanInDays)
            };
        }
    }
}
