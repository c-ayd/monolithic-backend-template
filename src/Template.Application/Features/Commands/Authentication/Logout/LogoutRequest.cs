using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Commands.Authentication.Logout
{
    public class LogoutRequest : IAsyncRequest<ExecResult<LogoutResponse>>
    {
        public bool? LogoutAllDevices { get; set; }
    }
}
