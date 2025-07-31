using FluentValidation;
using Template.Application.Localization;

namespace Template.Application.Features.Commands.Authentication.DeleteLogin
{
    public class DeleteLoginValidation : AbstractValidator<DeleteLoginRequest>
    {
        public DeleteLoginValidation()
        {
            RuleFor(_ => _.Id)
                .Cascade(CascadeMode.Stop)
                .NotNull()
                    .WithMessage("ID cannot be null")
                    .WithErrorCode(CommonLocalizationKeys.InvalidId)
                .NotEqual(Guid.Empty)
                    .WithMessage("ID cannot be empty")
                    .WithErrorCode(CommonLocalizationKeys.InvalidId);
        }
    }
}
