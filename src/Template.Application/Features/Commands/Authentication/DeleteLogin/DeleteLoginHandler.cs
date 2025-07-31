using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.ServerError;
using Cayd.AspNetCore.FlexLog;
using MediatR;
using Template.Application.Abstractions.Http;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;

namespace Template.Application.Features.Commands.Authentication.DeleteLogin
{
    public class DeleteLoginHandler : IRequestHandler<DeleteLoginRequest, ExecResult<DeleteLoginResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestContext _requestContext;
        private readonly IFlexLogger<DeleteLoginHandler> _flexLogger;

        public DeleteLoginHandler(IUnitOfWork unitOfWork,
            IRequestContext requestContext,
            IFlexLogger<DeleteLoginHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _requestContext = requestContext;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<DeleteLoginResponse>> Handle(DeleteLoginRequest request, CancellationToken cancellationToken)
        {
            var affectedRows = await _unitOfWork.Logins.DeleteByIdAndUserIdAsync(request.Id!.Value, _requestContext.UserId!.Value);
            if (affectedRows == 0)
                return new ExecNotFound("The login does not exist", AuthenticationLocalizationKeys.DeleteLoginNotFound);

            if (affectedRows > 1)
            {
                // This should normally never happen
                _flexLogger.LogCritical($"{affectedRows} logins are deleted instead of 1.", new
                {
                    _requestContext.UserId,
                    LoginId = request.Id
                });

                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            return new DeleteLoginResponse();
        }
    }
}
