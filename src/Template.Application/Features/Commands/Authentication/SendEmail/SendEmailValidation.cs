using FluentValidation;
using Template.Application.Localization;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Commands.Authentication.SendEmail
{
    public class SendEmailValidation : AbstractValidator<SendEmailRequest>
    {
        public SendEmailValidation()
        {
            RuleFor(_ => _.Email)
                .Cascade(CascadeMode.Stop)
                .EmailValidation();

            RuleFor(_ => _.Purpose)
                .NotNull()
                    .WithMessage("Purpose is null")
                    .WithErrorCode(AuthenticationLocalizationKeys.SendEmailPurposeRequired)
                .IsInEnum()
                    .WithMessage("Purpose is out of range")
                    .WithErrorCode(AuthenticationLocalizationKeys.SendEmailPurposeOutOfRange);
        }
    }
}
