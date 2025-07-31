using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Queries.Admin.GetUsers
{
    public class GetUsersRequest : IRequest<ExecResult<GetUsersResponse>>
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
