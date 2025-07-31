using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.DeleteLogin
{
    public class DeleteLoginRequest : IRequest<ExecResult<DeleteLoginResponse>>
    {
        public Guid? Id { get; set; }
    }
}
