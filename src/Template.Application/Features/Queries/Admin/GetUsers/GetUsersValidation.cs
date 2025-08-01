using FluentValidation;
using Template.Application.Validations.Extensions;

namespace Template.Application.Features.Queries.Admin.GetUsers
{
    public class GetUsersValidation : AbstractValidator<GetUsersRequest>
    {
        public GetUsersValidation()
        {
            RuleFor(_ => _.Page)
                .Cascade(CascadeMode.Stop)
                .PageValidation();

            RuleFor(_ => _.PageSize)
                .Cascade(CascadeMode.Stop)
                .PageSizeValidation();
        }
    }
}
