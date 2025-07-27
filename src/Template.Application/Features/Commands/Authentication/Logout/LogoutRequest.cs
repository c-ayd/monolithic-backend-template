using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.Logout
{
    public class LogoutRequest : IRequest<ExecResult<LogoutResponse>>
    {
        public bool? LogoutAllDevices { get; set; }
    }
}
