using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Commands.Authentication.Login
{
    public class LoginRequest : IAsyncRequest<ExecResult<LoginResponse>>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
