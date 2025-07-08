using System.Security.Claims;
using Template.Application.Dtos.Authentication;

namespace Template.Application.Abstractions.Authentication
{
    public interface IJwt
    {
        JwtTokenDto GenerateJwtToken(ICollection<Claim>? claims);
    }
}
