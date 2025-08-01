using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.ServerError;
using MediatR;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;
using Template.Domain.Entities.UserManagement.Enums;

namespace Template.Application.Features.Commands.Authentication.VerifyEmail
{
    public class VerifyEmailHandler : IRequestHandler<VerifyEmailRequest, ExecResult<VerifyEmailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashing _hashing;

        public VerifyEmailHandler(IUnitOfWork unitOfWork,
            IHashing hashing)
        {
            _unitOfWork = unitOfWork;
            _hashing = hashing;
        }

        public async Task<ExecResult<VerifyEmailResponse>> Handle(VerifyEmailRequest request, CancellationToken cancellationToken)
        {
            var hashedTokenValue = _hashing.HashSha256(request.Token!);
            var token = await _unitOfWork.Tokens.GetByHashedValueAndPurposeAsync(hashedTokenValue, ETokenPurpose.EmailVerification);
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

            // Update email verification value of the account
            var securityState = await _unitOfWork.Users.GetSecurityStateByIdAsync(token.UserId);
            if (securityState == null)
            {
                await _unitOfWork.SaveChangesAsync();

                // This code should never be executed. When a user is soft deleted, its tokens should be deleted as well.
                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            // Check email verification state
            if (securityState.IsEmailVerified)
            {
                await _unitOfWork.SaveChangesAsync();

                // This code should never be executed. When an email address is verified, the related token should be deleted as well.
                return new ExecConflict("Email is already verified", AuthenticationLocalizationKeys.EmailAlreadyVerified);
            }

            securityState.IsEmailVerified = true;

            await _unitOfWork.SaveChangesAsync();

            return new VerifyEmailResponse();
        }
    }
}
