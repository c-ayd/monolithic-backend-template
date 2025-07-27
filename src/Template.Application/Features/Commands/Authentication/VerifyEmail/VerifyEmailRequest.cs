using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.VerifyEmail
{
    public class VerifyEmailRequest : IRequest<ExecResult<VerifyEmailResponse>>
    {
        public string? Token { get; set; }
    }
}
