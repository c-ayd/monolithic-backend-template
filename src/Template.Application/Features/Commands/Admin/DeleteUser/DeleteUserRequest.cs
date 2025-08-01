using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Admin.DeleteUser
{
    public class DeleteUserRequest : IRequest<ExecResult<DeleteUserResponse>>
    {
        public Guid? Id { get; set; }
    }
}
