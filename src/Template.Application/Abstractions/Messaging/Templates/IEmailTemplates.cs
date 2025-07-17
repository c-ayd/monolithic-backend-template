using Template.Application.Dtos.Messaging.Templates;

namespace Template.Application.Abstractions.Messaging.Templates
{
    public interface IEmailTemplates
    {
        EmailTemplateDto GetEmailVerificationTemplate(string token, int expirationTimeInHours);
        EmailTemplateDto GetPasswordResetTemplate(string token, int expirationTimeInHours);
    }
}
