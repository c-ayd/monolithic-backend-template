using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Commands.Authentication.UpdatePassword
{
    public class UpdatePasswordValidation : AbstractValidator<UpdatePasswordRequest>
    {
        public UpdatePasswordValidation()
        {
            RuleFor(_ => _.NewPassword)
                .Cascade(CascadeMode.Stop)
                .PasswordValidation();

            RuleFor(_ => _.Password)
                .Cascade(CascadeMode.Stop)
                .PasswordValidation();
        }
    }
}
