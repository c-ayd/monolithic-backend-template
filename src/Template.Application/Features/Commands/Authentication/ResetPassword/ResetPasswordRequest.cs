using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.ResetPassword
{
    public class ResetPasswordRequest : IRequest<ExecResult<ResetPasswordResponse>>
    {
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
        public bool? LogoutAllDevices { get; set; }
    }
}
