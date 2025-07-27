using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using MediatR;
using Template.Application.Abstractions.Http;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;

namespace Template.Application.Features.Commands.Authentication.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutRequest, ExecResult<LogoutResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestContext _requestContext;

        public LogoutHandler(IUnitOfWork unitOfWork,
            IRequestContext requestContext)
        {
            _unitOfWork = unitOfWork;
            _requestContext = requestContext;
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
                var login = await _unitOfWork.Logins.GetByUserIdAndRefreshTokenAsync(_requestContext.UserId.Value, _requestContext.RefreshToken);
                if (login == null)
                    return new LogoutResponse();

                _unitOfWork.Logins.Delete(login);
                await _unitOfWork.SaveChangesAsync();
            }

            return new LogoutResponse();
        }
    }
}
