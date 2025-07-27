using FluentValidation;
using Template.Application.Localization;

namespace Template.Application.Features.Commands.Authentication.VerifyEmail
{
    public class VerifyEmailValidation : AbstractValidator<VerifyEmailRequest>
    {
        public VerifyEmailValidation()
        {
            RuleFor(_ => _.Token)
                .NotEmpty()
                    .WithMessage("Token is null or empty")
                    .WithErrorCode(AuthenticationLocalizationKeys.VerifyEmailNoToken);
        }
    }
}
