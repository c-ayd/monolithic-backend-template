using Cayd.AspNetCore.ExecutionResult;
using MediatR;
using Template.Domain.Entities.UserManagement.Enums;

namespace Template.Application.Features.Commands.Authentication.SendEmail
{
    public class SendEmailRequest : IRequest<ExecResult<SendEmailResponse>>
    {
        public string? Email { get; set; }
        public ETokenPurpose? Purpose { get; set; }
    }
}
