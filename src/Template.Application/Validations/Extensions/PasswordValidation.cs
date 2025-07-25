using FluentValidation;
using Template.Application.Localization;
using Template.Application.Validations.Constants.Entities.UserManagement;

namespace Template.Application.Validations.Extensions
{
    public static partial class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string?> PasswordValidation<T>(this IRuleBuilderInitial<T, string?> rule)
            => rule
                .NotEmpty()
                    .WithMessage("Password is null or empty")
                    .WithErrorCode(AuthenticationLocalizationKeys.PasswordRequired)
                .Length(PasswordConstants.MinLength, PasswordConstants.MaxLength)
                    .WithMessage("Password must be between 10 and 100 characters")
                    .WithErrorCode(AuthenticationLocalizationKeys.PasswordLengthError)
                .Matches(PasswordConstants.PasswordRegex)
                    .WithMessage("Password must include at least one numeric and one alphanumeric character")
                    .WithErrorCode(AuthenticationLocalizationKeys.PasswordInvalid);
    }
}
