using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.Login
{
    public class LoginRequest : IRequest<ExecResult<LoginResponse>>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
