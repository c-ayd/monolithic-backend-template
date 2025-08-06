using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.ServerError;
using Cayd.AspNetCore.FlexLog;
using Cayd.AspNetCore.Mediator.Abstractions;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;
using Template.Domain.Entities.UserManagement.Enums;

namespace Template.Application.Features.Commands.Authentication.ResetPassword
{
    public class ResetPasswordHandler : IAsyncHandler<ResetPasswordRequest, ExecResult<ResetPasswordResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashing _hashing;
        private readonly IFlexLogger<ResetPasswordHandler> _flexLogger;

        public ResetPasswordHandler(IUnitOfWork unitOfWork,
            IHashing hashing,
            IFlexLogger<ResetPasswordHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _hashing = hashing;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<ResetPasswordResponse>> HandleAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            var hashedTokenValue = _hashing.HashSha256(request.Token!);
            var token = await _unitOfWork.Tokens.GetByHashedValueAndPurposeAsync(hashedTokenValue, ETokenPurpose.ResetPassword);
            if (token == null)
                return new ExecNotFound("Token does not exist", AuthenticationLocalizationKeys.TokenNotFound);

            // Delete the token after using it
            _unitOfWork.Tokens.Delete(token);

            // Expiration date check
            if (DateTime.UtcNow > token.ExpirationDate)
            {
                await _unitOfWork.SaveChangesAsync();

                return new ExecGone("Token is expired", AuthenticationLocalizationKeys.TokenExpired);
            }

            // Update password
            var securityState = await _unitOfWork.Users.GetSecurityStateByIdAsync(token.UserId);
            if (securityState == null)
            {
                await _unitOfWork.SaveChangesAsync();

                // This code should never be executed. When a user is soft deleted, its tokens should be deleted as well.
                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Set new password
                securityState.PasswordHashed = _hashing.HashPassword(request.NewPassword!);

                // Log out from all devices if checked
                if (request.LogoutAllDevices.HasValue && request.LogoutAllDevices.Value)
                {
                    await _unitOfWork.Logins.DeleteAllByUserIdAsync(token.UserId);
                }

                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                _flexLogger.LogError("Something happened while resetting password of a user.", exception);

                await transaction.RollbackAsync();

                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            return new ResetPasswordResponse();
        }
    }
}
