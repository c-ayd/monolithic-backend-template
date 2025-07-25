using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Commands.Authentication.Register
{
    public class RegisterValidation : AbstractValidator<RegisterRequest>
    {
        public RegisterValidation()
        {
            RuleFor(_ => _.Email)
                .Cascade(CascadeMode.Stop)
                .EmailValidation();

            RuleFor(_ => _.Password)
                .Cascade(CascadeMode.Stop)
                .PasswordValidation();
        }
    }
}
