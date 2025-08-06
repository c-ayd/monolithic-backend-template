using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Commands.Admin.DeleteUser
{
    public class DeleteUserRequest : IAsyncRequest<ExecResult<DeleteUserResponse>>
    {
        public Guid? Id { get; set; }
    }
}
