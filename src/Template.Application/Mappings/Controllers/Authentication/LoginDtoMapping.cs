using Template.Application.Dtos.Controllers.Authentication;
using Template.Application.Features.Commands.Authentication.Login;

namespace Template.Application.Mappings.Controllers.Authentication
{
    public static partial class AuthenticationMappings
    {
        public static LoginDto Map(LoginResponse response)
            => new LoginDto()
                {
                    AccessToken = response.AccessToken
                };
    }
}
