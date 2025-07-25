namespace Template.Application.Localization
{
    public static class AuthenticationLocalizationCodes
    {
        /** 
         * NOTE: These values are only placeholders. If your frontend is multilingual,
         * you can return localization codes from the backend to show corresponding
         * localized messages to end users.
         * 
         * If this class gets bigger over time, consider using partial classes to divide
         * const values into different code files for grouping and maintability.
         */

        public const string EmailRequired = "email_required";
        public const string EmailTooLong = "email_too_long";
        public const string EmailInvalid = "email_invalid";

        public const string PasswordRequired = "password_required";
        public const string PasswordInvalid = "password_invalid";
        public const string PasswordLengthError = "password_length_error";
    }
}
