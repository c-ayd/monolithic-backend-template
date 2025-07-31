using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Queries.Authentication.GetLogins
{
    public class GetLoginsValidation : AbstractValidator<GetLoginsRequest>
    {
        public GetLoginsValidation()
        {
            RuleFor(_ => _.Password)
                .Cascade(CascadeMode.Stop)
                .PasswordValidation();
        }
    }
}
