using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Queries.Admin.GetUser
{
    public class GetUserRequest : IRequest<ExecResult<GetUserResponse>>
    {
        public Guid? Id { get; set; }
    }
}
