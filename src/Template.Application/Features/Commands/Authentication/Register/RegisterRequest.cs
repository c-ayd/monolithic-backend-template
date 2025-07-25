using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.Register
{
    public class RegisterRequest : IRequest<ExecResult<RegisterResponse>>
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
