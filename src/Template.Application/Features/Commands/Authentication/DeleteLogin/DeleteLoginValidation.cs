using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Commands.Authentication.DeleteLogin
{
    public class DeleteLoginValidation : AbstractValidator<DeleteLoginRequest>
    {
        public DeleteLoginValidation()
        {
            RuleFor(_ => _.Id)
                .Cascade(CascadeMode.Stop)
                .GuidValidation();
        }
    }
}
