using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.UpdateEmail
{
    public class UpdateEmailRequest : IRequest<ExecResult<UpdateEmailResponse>>
    {
        public string? NewEmail { get; set; }
        public string? Password { get; set; }
    }
}
