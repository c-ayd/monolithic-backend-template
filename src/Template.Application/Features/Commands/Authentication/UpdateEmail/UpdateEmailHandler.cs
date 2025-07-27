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
using Template.Domain.Entities.UserManagement.Enums;
using Template.Domain.Entities.UserManagement;
using Template.Application.Abstractions.Messaging.Templates;
using Template.Application.Abstractions.Messaging;

namespace Template.Application.Features.Commands.Authentication.UpdateEmail
{
    public class UpdateEmailHandler : IRequestHandler<UpdateEmailRequest, ExecResult<UpdateEmailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRequestContext _requestContext;
        private readonly IHashing _hashing;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IEmailTemplates _emailTemplates;
        private readonly IEmailSender _emailSender;
        private readonly AccountLockSettings _accountLockSettings;
        private readonly TokenLifetimesSettings _tokenLifetimesSettings;
        private readonly IFlexLogger<UpdateEmailHandler> _flexLogger;

        public UpdateEmailHandler(IUnitOfWork unitOfWork,
            IRequestContext requestContext,
            IHashing hashing,
            ITokenGenerator tokenGenerator,
            IEmailTemplates emailTemplates,
            IEmailSender emailSender,
            IOptions<AccountLockSettings> accountLockSettings,
            IOptions<TokenLifetimesSettings> tokenLifetimesSettings,
            IFlexLogger<UpdateEmailHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _requestContext = requestContext;
            _hashing = hashing;
            _tokenGenerator = tokenGenerator;
            _emailTemplates = emailTemplates;
            _emailSender = emailSender;
            _accountLockSettings = accountLockSettings.Value;
            _tokenLifetimesSettings = tokenLifetimesSettings.Value;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<UpdateEmailResponse>> Handle(UpdateEmailRequest request, CancellationToken cancellationToken)
        {
            var user = await _unitOfWork.Users.GetByIdWithSecurityStateAsync(_requestContext.UserId!.Value);
            if (user == null || user.SecurityState == null)
                // This code is executed when user is soft deleted but its access token is still valid for a short period time
                // or the JWT secret key is compromised and someone is generating access tokens. If it is the former,
                // blacklisting JWT tokens after users are soft deleted can solve this.
                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);

            // Compare passwords
            var passwordVerificationResult = _hashing.VerifyPassword(user.SecurityState.PasswordHashed!, request.Password!);
            switch (passwordVerificationResult)
            {
                case EPasswordVerificationResult.Fail:
                    ++user.SecurityState.FailedAttempts;
                    if (user.SecurityState.FailedAttempts >= _accountLockSettings.FailedAttemptsForSecondLock)
                    {
                        user.SecurityState.FailedAttempts = 0;
                        user.SecurityState.IsLocked = true;
                        user.SecurityState.UnlockDate = DateTime.UtcNow.AddMinutes(_accountLockSettings.SecondLockTimeInMinutes);

                        _flexLogger.LogWarning("An account is locked.", new
                        {
                            UserId = user.Id
                        });
                    }
                    else if (user.SecurityState.FailedAttempts >= _accountLockSettings.FailedAttemptsForFirstLock)
                    {
                        user.SecurityState.IsLocked = true;
                        user.SecurityState.UnlockDate = DateTime.UtcNow.AddMinutes(_accountLockSettings.FirstLockTimeInMinutes);
                    }
                    else
                    {
                        await _unitOfWork.SaveChangesAsync();

                        return new ExecErrorDetail("The credentials are wrong", AuthenticationLocalizationKeys.LoginWrongCredentials);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    return new ExecLocked($"The account is locked due to {user.SecurityState.FailedAttempts} failed attemps",
                        AuthenticationLocalizationKeys.LoginLocked,
                        new
                        {
                            user.SecurityState.FailedAttempts,
                            user.SecurityState.UnlockDate
                        });
                case EPasswordVerificationResult.VersionNotFound:
                    _flexLogger.LogError("Password verification failed due to the version difference.", new
                    {
                        PasswordVersion = Convert.FromBase64String(user.SecurityState.PasswordHashed!)[0]
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
                case EPasswordVerificationResult.LengthMismatch:
                    _flexLogger.LogError("Passwords' lengths do not match.", new
                    {
                        PasswordVersion = Convert.FromBase64String(user.SecurityState.PasswordHashed!)[0]
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
                case EPasswordVerificationResult.SuccessRehashNeeded:
                    user.SecurityState.FailedAttempts = 0;
                    user.SecurityState.PasswordHashed = _hashing.HashPassword(request.Password!);
                    break;
                case EPasswordVerificationResult.Success:
                    user.SecurityState.FailedAttempts = 0;
                    break;
                default:
                    _flexLogger.LogError("Password verification result is outside of the enum range.", new
                    {
                        VerificationResult = passwordVerificationResult
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Set new email address
                user.Email = request.NewEmail;

                // Revoke all tokens related to user
                await _unitOfWork.Tokens.DeleteAllByUserIdAsync(user.Id);

                // Generate a new email verification token
                var emailVerificationTokenValue = _tokenGenerator.GenerateBase64UrlSafe();
                var emailVerificationExpirationTimeInHours = _tokenLifetimesSettings.EmailVerificationLifetimeInHours;
                var emailVerificationToken = new Token()
                {
                    Value = _hashing.HashSha256(emailVerificationTokenValue),
                    Purpose = ETokenPurpose.EmailVerification,
                    ExpirationDate = DateTime.UtcNow.AddHours(emailVerificationExpirationTimeInHours),
                    User = user
                };
                await _unitOfWork.Tokens.AddAsync(emailVerificationToken);
                await _unitOfWork.SaveChangesAsync();

                // Send a verification email
                var emailTemplate = _emailTemplates.GetEmailVerificationTemplate(emailVerificationTokenValue, emailVerificationExpirationTimeInHours);
                await _emailSender.SendAsync(request.NewEmail!, emailTemplate.Subject!, emailTemplate.Body!, isBodyHtml: false); // NOTE: If the expected template is HTML, switch it to 'true'

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                _flexLogger.LogError("Something happened while updating email address of a user.", exception);

                await transaction.RollbackAsync();

                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            return new UpdateEmailResponse();
        }
    }
}
