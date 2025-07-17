using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Template.Application.Abstractions.Messaging.Templates;
using Template.Application.Dtos.Messaging.Templates;
using Template.Infrastructure.Settings.Messaging.Templates;

namespace Template.Infrastructure.Messaging.Templates
{
    public class EmailTemplates : IEmailTemplates
    {
        private readonly IStringLocalizer<EmailTemplates> _localizer;
        private readonly TemplateLinksSettings _templateLinksSettings;

        public EmailTemplates(IStringLocalizer<EmailTemplates> localizer,
            IOptions<TemplateLinksSettings> templateLinksSettings)
        {
            _localizer = localizer;
            _templateLinksSettings = templateLinksSettings.Value;
        }

        public EmailTemplateDto GetEmailVerificationTemplate(string token, int expirationTimeInHours)
        {
            var subject = _localizer["EmailVerificationSubject"];
            var body = string.Format(_localizer["EmailVerificationBody"],
                _templateLinksSettings.EmailVerification + token,
                expirationTimeInHours);

            return new EmailTemplateDto()
            {
                Subject = subject,
                Body = body
            };
        }

        public EmailTemplateDto GetPasswordResetTemplate(string token, int expirationTimeInHours)
        {
            var subject = _localizer["ResetPasswordSubject"];
            var body = string.Format(_localizer["ResetPasswordBody"],
                _templateLinksSettings.ResetPassword + token,
                expirationTimeInHours);

            return new EmailTemplateDto()
            {
                Subject = subject,
                Body = body
            };
        }
    }
}
