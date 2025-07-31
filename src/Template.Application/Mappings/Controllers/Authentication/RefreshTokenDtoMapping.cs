using Template.Application.Dtos.Controllers.Authentication;
using Template.Application.Features.Commands.Authentication.RefreshToken;

namespace Template.Application.Mappings.Controllers.Authentication
{
    public static partial class AuthenticationMappings
    {
        public static RefreshTokenDto Map(RefreshTokenResponse response)
            => new RefreshTokenDto()
                {
                    AccessToken = response.AccessToken
                };
    }
}
