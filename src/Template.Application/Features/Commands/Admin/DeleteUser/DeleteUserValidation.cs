using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Commands.Admin.DeleteUser
{
    public class DeleteUserValidation : AbstractValidator<DeleteUserRequest>
    {
        public DeleteUserValidation()
        {
            RuleFor(_ => _.Id)
                .Cascade(CascadeMode.Stop)
                .GuidValidation();
        }
    }
}
