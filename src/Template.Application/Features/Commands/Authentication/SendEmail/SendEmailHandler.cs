using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.ServerError;
using Cayd.AspNetCore.FlexLog;
using MediatR;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.Messaging;
using Template.Application.Abstractions.Messaging.Templates;
using Template.Application.Abstractions.UOW;
using Template.Application.Dtos.Messaging.Templates;
using Template.Application.Localization;
using Template.Application.Settings;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Entities.UserManagement.Enums;

namespace Template.Application.Features.Commands.Authentication.SendEmail
{
    public class SendEmailHandler : IRequestHandler<SendEmailRequest, ExecResult<SendEmailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly TokenLifetimesSettings _tokenLifetimesSettings;
        private readonly IHashing _hashing;
        private readonly IEmailTemplates _emailTemplates;
        private readonly IEmailSender _emailSender;
        private readonly IFlexLogger<SendEmailHandler> _flexLogger;

        public SendEmailHandler(IUnitOfWork unitOfWork,
            ITokenGenerator tokenGenerator,
            IOptions<TokenLifetimesSettings> tokenLifetimesSettings,
            IHashing hashing,
            IEmailTemplates emailTemplates,
            IEmailSender emailSender,
            IFlexLogger<SendEmailHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _tokenGenerator = tokenGenerator;
            _tokenLifetimesSettings = tokenLifetimesSettings.Value;
            _hashing = hashing;
            _emailTemplates = emailTemplates;
            _emailSender = emailSender;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<SendEmailResponse>> Handle(SendEmailRequest request, CancellationToken cancellationToken)
        {
            var securityState = await _unitOfWork.Users.GetSecurityStateByEmailAsync(request.Email!);
            if (securityState == null)
                // In order to not disclosure which email addresses are in the database,
                // even if the email does not exist in the database, a success code is returned.
                return new SendEmailResponse();

            // The purpose check of the request
            string tokenValue;
            DateTime expirationDate;
            EmailTemplateDto emailTemplate;
            switch (request.Purpose!.Value)
            {
                case ETokenPurpose.EmailVerification:
                    if (securityState.IsEmailVerified)
                        return new ExecConflict("The email is already verified", AuthenticationLocalizationKeys.SendEmailVerificationOfEmailIsAlreadyDone);

                    tokenValue = _tokenGenerator.GenerateBase64UrlSafe();
                    expirationDate = DateTime.UtcNow.AddHours(_tokenLifetimesSettings.EmailVerificationLifetimeInHours);
                    emailTemplate = _emailTemplates.GetEmailVerificationTemplate(tokenValue, _tokenLifetimesSettings.EmailVerificationLifetimeInHours);
                    break;
                case ETokenPurpose.ResetPassword:
                    tokenValue = _tokenGenerator.GenerateBase64UrlSafe();
                    expirationDate = DateTime.UtcNow.AddHours(_tokenLifetimesSettings.ResetPasswordLifetimeInHours);
                    emailTemplate = _emailTemplates.GetPasswordResetTemplate(tokenValue, _tokenLifetimesSettings.ResetPasswordLifetimeInHours);
                    break;
                default:
                    _flexLogger.LogError("Email purpose is outside of the enum range.", new
                    {
                        request.Purpose
                    });
                    return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            // Token creation
            var token = new Token()
            {
                ValueHashed = _hashing.HashSha256(tokenValue),
                Purpose = request.Purpose.Value,
                ExpirationDate = expirationDate,
                UserId = securityState.UserId,
            };

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Deleting other same tokens
                await _unitOfWork.Tokens.DeleteAllByUserIdAndPurposeAsync(securityState.UserId, request.Purpose.Value);

                // Saving token to the database
                await _unitOfWork.Tokens.AddAsync(token);
                await _unitOfWork.SaveChangesAsync();

                // Sending email
                await _emailSender.SendAsync(request.Email!, emailTemplate.Subject!, emailTemplate.Body!, isBodyHtml: false); // NOTE: If the expected template is HTML, switch it to 'true'

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();

                _flexLogger.LogError(exception.Message, exception);

                return new ExecInternalServerError("Something went wrong", CommonLocalizationKeys.InternalServerError);
            }

            return new SendEmailResponse();
        }
    }
}
