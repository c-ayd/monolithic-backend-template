using FluentValidation;
using Template.Application.Localization;
using Template.Application.Validations.Constants.Entities.UserManagement;

namespace Template.Application.Validations.Extensions
{
    public static partial class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string?> EmailValidation<T>(this IRuleBuilderInitial<T, string?> rule)
            => rule
                .NotEmpty()
                    .WithMessage("Email is null or empty")
                    .WithErrorCode(AuthenticationLocalizationCodes.EmailRequired)
                .MaximumLength(UserConstants.EmailMaxLength)
                    .WithMessage("Email address is too long")
                    .WithErrorCode(AuthenticationLocalizationCodes.EmailTooLong)
                .Matches(UserConstants.EmailRegex)
                    .WithMessage("Email is invalid")
                    .WithErrorCode(AuthenticationLocalizationCodes.EmailInvalid);
    }
}
