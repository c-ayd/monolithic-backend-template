using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.ServerError;
using Cayd.AspNetCore.FlexLog;
using Cayd.AspNetCore.Mediator.Abstractions;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;

namespace Template.Application.Features.Commands.Admin.DeleteUser
{
    public class DeleteUserHandler : IAsyncHandler<DeleteUserRequest, ExecResult<DeleteUserResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFlexLogger<DeleteUserHandler> _flexLogger;

        public DeleteUserHandler(IUnitOfWork unitOfWork,
            IFlexLogger<DeleteUserHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<DeleteUserResponse>> HandleAsync(DeleteUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdWithSecurityStateAsync(request.Id!.Value);
            if (user == null)
                return new ExecNotFound("User is not found", AdminLocalizationKeys.UserNotFound);

            if (user.IsDeleted)
                // This code should never be executed since the query above will return null if the user is soft deleted.
                return new ExecConflict("User is already soft deleted", AdminLocalizationKeys.UserAlreadySoftDeleted);

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Tokens.DeleteAllByUserIdAsync(user.Id);
                await _unitOfWork.Logins.DeleteAllByUserIdAsync(user.Id);

                user.Email = null;
                user.SecurityState!.PasswordHashed = null;

                _unitOfWork.Users.Delete(user);

                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();

                _flexLogger.LogError(exception.Message, exception);

                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            return new DeleteUserResponse();
        }
    }
}
