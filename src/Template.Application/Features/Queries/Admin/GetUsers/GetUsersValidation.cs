using FluentValidation;
using Template.Application.Localization;
using Template.Application.Validations.Constants;

namespace Template.Application.Features.Queries.Admin.GetUsers
{
    public class GetUsersValidation : AbstractValidator<GetUsersRequest>
    {
        public GetUsersValidation()
        {
            RuleFor(_ => _.Page)
                .GreaterThan(0)
                    .WithMessage("The page number must be positive")
                    .WithErrorCode(CommonLocalizationKeys.PaginationPositiveNumber);

            RuleFor(_ => _.PageSize)
                .LessThanOrEqualTo(PaginationConstants.MaxPageSize)
                    .WithMessage("The page size is too large")
                    .WithErrorCode(CommonLocalizationKeys.PaginationSizeLimit);
        }
    }
}
