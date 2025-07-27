using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Api.Utilities;
using Template.Application.Features.Commands.Authentication.Login;
using Template.Application.Features.Commands.Authentication.Logout;
using Template.Application.Features.Commands.Authentication.Register;
using Template.Application.Features.Commands.Authentication.UpdateEmail;
using Template.Application.Features.Commands.Authentication.VerifyEmail;
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

        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout([FromQuery] bool? logoutAllDevices = false)
        {
            var result = await _sender.Send(new LogoutRequest()
            {
                LogoutAllDevices = logoutAllDevices
            });

            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpPatch("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            var result = await _sender.Send(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [Authorize]
        [HttpPatch("update-email")]
        public async Task<IActionResult> UpdateEmail(UpdateEmailRequest request)
        {
            var result = await _sender.Send(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }
    }
}
