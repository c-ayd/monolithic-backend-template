using System.Security.Claims;
using Template.Application.Dtos.Authentication;

namespace Template.Application.Abstractions.Authentication
{
    public interface IJwt
    {
        JwtTokenDto GenerateJwtToken();
        JwtTokenDto GenerateJwtToken(ICollection<Claim> claims);
        JwtTokenDto GenerateJwtToken(ICollection<Claim> claims, DateTime notBefore);
        JwtTokenDto GenerateJwtToken(DateTime notBefore);
    }
}
