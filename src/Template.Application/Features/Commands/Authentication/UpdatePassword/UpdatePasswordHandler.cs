using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.ServerError;
using Cayd.AspNetCore.FlexLog;
using MediatR;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.Http;
using Template.Application.Abstractions.UOW;
using Template.Application.Dtos.Crypto.Enums;
using Template.Application.Localization;
using Template.Application.Settings;

namespace Template.Application.Features.Commands.Authentication.UpdatePassword
{
    public class UpdatePasswordHandler : IRequestHandler<UpdatePasswordRequest, ExecResult<UpdatePasswordResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestContext _requestContext;
        private readonly IHashing _hashing;
        private readonly AccountLockSettings _accountLockSettings;
        private readonly IFlexLogger<UpdatePasswordHandler> _flexLogger;

        public UpdatePasswordHandler(IUnitOfWork unitOfWork,
            IRequestContext requestContext,
            IHashing hashing,
            IOptions<AccountLockSettings> accountLockSettings,
            IFlexLogger<UpdatePasswordHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _requestContext = requestContext;
            _hashing = hashing;
            _accountLockSettings = accountLockSettings.Value;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<UpdatePasswordResponse>> Handle(UpdatePasswordRequest request, CancellationToken cancellationToken)
        {
            var securityState = await _unitOfWork.Users.GetSecurityStateByIdAsync(_requestContext.UserId!.Value);
            if (securityState == null)
                // This code is executed when user is soft deleted but its access token is still valid for a short period time
                // or the JWT secret key is compromised and someone is generating access tokens. If it is the former,
                // blacklisting JWT tokens after users are soft deleted can solve this.
                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);

            // Compare passwords
            var passwordVerificationResult = _hashing.VerifyPassword(securityState.PasswordHashed!, request.Password!);
            switch (passwordVerificationResult)
            {
                case EPasswordVerificationResult.Fail:
                    ++securityState.FailedAttempts;
                    if (securityState.FailedAttempts >= _accountLockSettings.FailedAttemptsForSecondLock)
                    {
                        securityState.FailedAttempts = 0;
                        securityState.IsLocked = true;
                        securityState.UnlockDate = DateTime.UtcNow.AddMinutes(_accountLockSettings.SecondLockTimeInMinutes);

                        _flexLogger.LogWarning("An account is locked.", new
                        {
                            securityState.UserId
                        });
                    }
                    else if (securityState.FailedAttempts >= _accountLockSettings.FailedAttemptsForFirstLock)
                    {
                        securityState.IsLocked = true;
                        securityState.UnlockDate = DateTime.UtcNow.AddMinutes(_accountLockSettings.FirstLockTimeInMinutes);
                    }
                    else
                    {
                        await _unitOfWork.SaveChangesAsync();

                        return new ExecErrorDetail("The credentials are wrong", AuthenticationLocalizationKeys.LoginWrongCredentials);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    return new ExecLocked($"The account is locked due to {securityState.FailedAttempts} failed attemps",
                        AuthenticationLocalizationKeys.LoginLocked,
                        new
                        {
                            securityState.FailedAttempts,
                            securityState.UnlockDate
                        });
                case EPasswordVerificationResult.VersionNotFound:
                    _flexLogger.LogError("Password verification failed due to the version difference.", new
                    {
                        PasswordVersion = Convert.FromBase64String(securityState.PasswordHashed!)[0]
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
                case EPasswordVerificationResult.LengthMismatch:
                    _flexLogger.LogError("Passwords' lengths do not match.", new
                    {
                        PasswordVersion = Convert.FromBase64String(securityState.PasswordHashed!)[0]
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
                case EPasswordVerificationResult.SuccessRehashNeeded:
                case EPasswordVerificationResult.Success:
                    securityState.FailedAttempts = 0;
                    break;
                default:
                    _flexLogger.LogError("Password verification result is outside of the enum range.", new
                    {
                        VerificationResult = passwordVerificationResult
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            // Update password
            securityState.PasswordHashed = _hashing.HashPassword(request.NewPassword!);

            await _unitOfWork.SaveChangesAsync();

            return new UpdatePasswordResponse();
        }
    }
}
