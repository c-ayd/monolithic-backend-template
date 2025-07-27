using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Commands.Authentication.UpdateEmail
{
    public class UpdateEmailValidation : AbstractValidator<UpdateEmailRequest>
    {
        public UpdateEmailValidation()
        {
            RuleFor(_ => _.NewEmail)
                .Cascade(CascadeMode.Stop)
                .EmailValidation();

            RuleFor(_ => _.Password)
                .Cascade(CascadeMode.Stop)
                .PasswordValidation();
        }
    }
}
