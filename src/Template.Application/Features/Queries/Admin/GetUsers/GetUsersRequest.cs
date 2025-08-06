using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.Mediator.Abstractions;

namespace Template.Application.Features.Queries.Admin.GetUsers
{
    public class GetUsersRequest : IAsyncRequest<ExecResult<GetUsersResponse>>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
