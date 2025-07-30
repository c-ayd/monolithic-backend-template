using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.UpdatePassword
{
    public class UpdatePasswordRequest : IRequest<ExecResult<UpdatePasswordResponse>>
    {
        public string? NewPassword { get; set; }
        public string? Password { get; set; }
    }
}
