using MediatR;
using Microsoft.AspNetCore.Mvc;
using Template.Api.Utilities;
using Template.Application.Features.Commands.Authentication.Login;
using Template.Application.Features.Commands.Authentication.Register;
using Template.Application.Mappings;

namespace Template.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ISender _sender;

        public AuthenticationController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _sender.Send(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _sender.Send(request);
            return result.Match(
                (code, response, metadata) =>
                {
                    HttpContext.Response.Cookies.AddRefreshToken(response.RefreshToken, response.RefreshTokenExpirationDate);
                    return JsonUtility.Success(code, AuthenticationMappings.LoginMapping(response), metadata);
                },
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }
    }
}
