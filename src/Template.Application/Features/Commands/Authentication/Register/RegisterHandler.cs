using Cayd.AspNetCore.ExecutionResult;
using Cayd.AspNetCore.ExecutionResult.ClientError;
using Cayd.AspNetCore.ExecutionResult.Success;
using Cayd.AspNetCore.FlexLog;
using MediatR;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.Messaging;
using Template.Application.Abstractions.Messaging.Templates;
using Template.Application.Abstractions.UOW;
using Template.Application.Localization;
using Template.Application.Settings;
using Template.Domain.Entities.UserManagement;
using Template.Domain.Entities.UserManagement.Enums;

namespace Template.Application.Features.Commands.Authentication.Register
{
    public class RegisterHandler : IRequestHandler<RegisterRequest, ExecResult<RegisterResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHashing _hashing;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly TokenLifetimesSettings _tokenLifetimesSettings;
        private readonly IEmailTemplates _emailTemplates;
        private readonly IEmailSender _emailSender;
        private readonly IFlexLogger<RegisterHandler> _flexLogger;

        public RegisterHandler(IUnitOfWork unitOfWork,
            IHashing hashing,
            ITokenGenerator tokenGenerator,
            IOptions<TokenLifetimesSettings> tokenLifetimesSettings,
            IEmailTemplates emailTemplates,
            IEmailSender emailSender,
            IFlexLogger<RegisterHandler> flexLogger)
        {
            _unitOfWork = unitOfWork;
            _hashing = hashing;
            _tokenGenerator = tokenGenerator;
            _tokenLifetimesSettings = tokenLifetimesSettings.Value;
            _emailTemplates = emailTemplates;
            _emailSender = emailSender;
            _flexLogger = flexLogger;
        }

        public async Task<ExecResult<RegisterResponse>> Handle(RegisterRequest request, CancellationToken cancellationToken)
        {
            // Check if the user exists
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email!);
            if (user != null)
                return new ExecConflict("This email address already exists", AuthenticationLocalizationKeys.RegisterEmailExists);

            // Add a new user to the database
            var newUser = new User()
            {
                Email = request.Email,
                SecurityState = new SecurityState()
                {
                    PasswordHashed = _hashing.HashPassword(request.Password!)
                }
            };
            await _unitOfWork.Users.AddAsync(newUser);

            // Add a new email verification token to the database
            var emailVerificationTokenValue = _tokenGenerator.GenerateBase64UrlSafe();
            var emailVerificationExpirationTimeInHours = _tokenLifetimesSettings.EmailVerificationLifetimeInHours;
            var emailVerificationToken = new Token()
            {
                Value = _hashing.HashSha256(emailVerificationTokenValue),
                Purpose = ETokenPurpose.EmailVerification,
                ExpirationDate = DateTime.UtcNow.AddHours(emailVerificationExpirationTimeInHours),
                User = newUser
            };
            await _unitOfWork.Tokens.AddAsync(emailVerificationToken);

            // Save the new user and email verification token to the database
            await _unitOfWork.SaveChangesAsync();

            // Send a verification email
            var emailTemplate = _emailTemplates.GetEmailVerificationTemplate(emailVerificationTokenValue, emailVerificationExpirationTimeInHours);
            try
            {
                await _emailSender.SendAsync(request.Email!, emailTemplate.Subject!, emailTemplate.Body!, isBodyHtml: false); // NOTE: If the expected template is HTML, switch it to 'true'
            }
            catch (Exception exception)
            {
                _flexLogger.LogError(exception.Message, exception);

                // The user is added to the database, however, the email could not be sent.
                return new ExecMultiStatus<RegisterResponse>(new RegisterResponse()
                {
                    UserId = newUser.Id
                },
                new
                {
                    Status = "The user has been created, but the verification email could not be sent",
                    LocalizationCode = AuthenticationLocalizationKeys.RegisterSucceededButSendingEmailFailed
                });
            }
            
            return new RegisterResponse()
            {
                UserId = newUser.Id
            };
        }
    }
}
