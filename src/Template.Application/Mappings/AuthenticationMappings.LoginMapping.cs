using Template.Application.Dtos.Controllers.Authentication;
using Template.Application.Features.Commands.Authentication.Login;

namespace Template.Application.Mappings
{
    public static partial class AuthenticationMappings
    {
        public static LoginDto LoginMapping(LoginResponse response)
            => new LoginDto()
                {
                    AccessToken = response.AccessToken
                };
    }
}
