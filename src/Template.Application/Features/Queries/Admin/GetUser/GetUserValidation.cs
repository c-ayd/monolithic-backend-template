using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Queries.Admin.GetUser
{
    public class GetUserValidation : AbstractValidator<GetUserRequest>
    {
        public GetUserValidation()
        {
            RuleFor(_ => _.Id)
                .Cascade(CascadeMode.Stop)
                .GuidValidation();
        }
    }
}
