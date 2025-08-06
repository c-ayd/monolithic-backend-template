using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Commands.Authentication.ResetPassword
{
    public class ResetPasswordRequest : IAsyncRequest<ExecResult<ResetPasswordResponse>>
    {
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
        public bool? LogoutAllDevices { get; set; }
    }
}
