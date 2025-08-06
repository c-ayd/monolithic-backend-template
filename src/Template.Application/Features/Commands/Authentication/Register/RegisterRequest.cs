using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Commands.Authentication.Register
{
    public class RegisterRequest : IAsyncRequest<ExecResult<RegisterResponse>>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
