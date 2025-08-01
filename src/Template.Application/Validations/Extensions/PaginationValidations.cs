using FluentValidation;
using Template.Application.Localization;
using Template.Application.Validations.Constants;

namespace Template.Application.Validations.Extensions
{
    public static partial class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, int?> PageValidation<T>(this IRuleBuilderInitial<T, int?> rule)
            => rule
                .GreaterThan(0)
                    .WithMessage("The page number must be positive")
                    .WithErrorCode(CommonLocalizationKeys.PaginationZeroOrNegative);

        public static IRuleBuilderOptions<T, int?> PageSizeValidation<T>(this IRuleBuilderInitial<T, int?> rule)
            => rule
                .InclusiveBetween(1, PaginationConstants.MaxPageSize)
                    .WithMessage($"The page size must be at least 1 and a maximum of {PaginationConstants.MaxPageSize}")
                    .WithErrorCode(CommonLocalizationKeys.PaginationSizeLimit);
    }
}
