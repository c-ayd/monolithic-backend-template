using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Commands.Authentication.UpdatePassword
{
    public class UpdatePasswordRequest : IAsyncRequest<ExecResult<UpdatePasswordResponse>>
    {
        public string? NewPassword { get; set; }
        public string? Password { get; set; }
    }
}
