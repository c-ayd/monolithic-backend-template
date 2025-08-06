using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Commands.Authentication.UpdateEmail
{
    public class UpdateEmailRequest : IAsyncRequest<ExecResult<UpdateEmailResponse>>
    {
        public string? NewEmail { get; set; }
        public string? Password { get; set; }
    }
}
