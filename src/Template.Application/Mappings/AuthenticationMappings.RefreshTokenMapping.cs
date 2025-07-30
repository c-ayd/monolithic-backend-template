using Template.Application.Dtos.Controllers.Authentication;
using Template.Application.Features.Commands.Authentication.RefreshToken;

namespace Template.Application.Mappings
{
    public static partial class AuthenticationMappings
    {
        public static RefreshTokenDto RefreshTokenMapping(RefreshTokenResponse response)
            => new RefreshTokenDto()
                {
                    AccessToken = response.AccessToken
                };
    }
}
