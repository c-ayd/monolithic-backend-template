using Cayd.AspNetCore.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Template.Api.Utilities;
using Template.Application.Features.Commands.Authentication.DeleteLogin;
using Template.Application.Features.Commands.Authentication.Login;
using Template.Application.Features.Commands.Authentication.Logout;
using Template.Application.Features.Commands.Authentication.RefreshToken;
using Template.Application.Features.Commands.Authentication.Register;
using Template.Application.Features.Commands.Authentication.ResetPassword;
using Template.Application.Features.Commands.Authentication.SendEmail;
using Template.Application.Features.Commands.Authentication.UpdateEmail;
using Template.Application.Features.Commands.Authentication.UpdatePassword;
using Template.Application.Features.Commands.Authentication.VerifyEmail;
using Template.Application.Features.Queries.Authentication.GetLogins;
using Template.Application.Mappings.Controllers.Authentication;

namespace Template.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthenticationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _mediator.SendAsync(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var result = await _mediator.SendAsync(request);
            return result.Match(
                (code, response, metadata) =>
                {
                    HttpContext.Response.Cookies.AddRefreshToken(response.RefreshToken, response.RefreshTokenExpirationDate);
                    return JsonUtility.Success(code, AuthenticationMappings.Map(response), metadata);
                },
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [Authorize]
        [HttpDelete("logout")]
        public async Task<IActionResult> Logout([FromQuery] bool? logoutAllDevices = false)
        {
            var result = await _mediator.SendAsync(new LogoutRequest()
            {
                LogoutAllDevices = logoutAllDevices
            });

            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request)
        {
            var result = await _mediator.SendAsync(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [Authorize]
        [HttpPost("update-email")]
        public async Task<IActionResult> UpdateEmail(UpdateEmailRequest request)
        {
            var result = await _mediator.SendAsync(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            var result = await _mediator.SendAsync(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [Authorize]
        [HttpPatch("update-password")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest request)
        {
            var result = await _mediator.SendAsync(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail(SendEmailRequest request)
        {
            var result = await _mediator.SendAsync(request);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var result = await _mediator.SendAsync(new RefreshTokenRequest());
            return result.Match(
                (code, response, metadata) =>
                {
                    HttpContext.Response.Cookies.AddRefreshToken(response.RefreshToken, response.RefreshTokenExpirationDate);
                    return JsonUtility.Success(code, AuthenticationMappings.Map(response), metadata);
                },
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [Authorize]
        [HttpPost("logins")]
        public async Task<IActionResult> GetLogins(GetLoginsRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.SendAsync(request, cancellationToken);
            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, AuthenticationMappings.Map(response), metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }

        [Authorize]
        [HttpDelete("logins/{id}")]
        public async Task<IActionResult> DeleteLogin(Guid? id)
        {
            var result = await _mediator.SendAsync(new DeleteLoginRequest()
            {
                Id = id
            });

            return result.Match(
                (code, response, metadata) => JsonUtility.Success(code, metadata),
                (code, errors, metadata) => JsonUtility.Fail(code, errors, metadata)
            );
        }
    }
}
