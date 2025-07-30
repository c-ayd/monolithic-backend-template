using FluentValidation;
using Template.Application.Localization;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Commands.Authentication.ResetPassword
{
    public class ResetPasswordValidation : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordValidation()
        {
            RuleFor(_ => _.Token)
                .NotEmpty()
                    .WithMessage("Token is null or empty")
                    .WithErrorCode(AuthenticationLocalizationKeys.TokenEmpty);

            RuleFor(_ => _.NewPassword)
                .Cascade(CascadeMode.Stop)
                .PasswordValidation();
        }
    }
}
