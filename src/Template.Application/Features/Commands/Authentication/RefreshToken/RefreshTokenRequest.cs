using Cayd.AspNetCore.ExecutionResult;
using MediatR;

namespace Template.Application.Features.Commands.Authentication.RefreshToken
{
    public class RefreshTokenRequest : IRequest<ExecResult<RefreshTokenResponse>>
    {
    }
}
