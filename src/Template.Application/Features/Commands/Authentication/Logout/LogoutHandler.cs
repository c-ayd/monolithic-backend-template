using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using MediatR;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.Http;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;

namespace Template.Application.Features.Commands.Authentication.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutRequest, ExecResult<LogoutResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestContext _requestContext;
        private readonly IHashing _hashing;

        public LogoutHandler(IUnitOfWork unitOfWork,
            IRequestContext requestContext,
            IHashing hashing)
        {
            _unitOfWork = unitOfWork;
            _requestContext = requestContext;
            _hashing = hashing;
        }

        public async Task<ExecResult<LogoutResponse>> Handle(LogoutRequest request, CancellationToken cancellationToken)
        {
            if (_requestContext.UserId == null || _requestContext.RefreshToken == null)
                return new ExecUnauthorized("Unauthorized", AuthenticationLocalizationKeys.LogoutUnauthorized);

            if (request.LogoutAllDevices != null && request.LogoutAllDevices.Value)
            {
                await _unitOfWork.Logins.DeleteAllByUserIdAsync(_requestContext.UserId.Value);
            }
            else
            {
                var hashedRefreshToken = _hashing.HashSha256(_requestContext.RefreshToken);
                var login = await _unitOfWork.Logins.GetByUserIdAndHashedRefreshTokenAsync(_requestContext.UserId.Value, hashedRefreshToken);
                if (login == null)
                    return new LogoutResponse();

                _unitOfWork.Logins.Delete(login);
                await _unitOfWork.SaveChangesAsync();
            }

            return new LogoutResponse();
        }
    }
}
