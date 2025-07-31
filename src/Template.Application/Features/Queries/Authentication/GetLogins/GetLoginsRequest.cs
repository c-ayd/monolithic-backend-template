using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Queries.Authentication.GetLogins
{
    public class GetLoginsRequest : IRequest<ExecResult<GetLoginsResponse>>
    {
        public string? Password { get; set; }
    }
}
