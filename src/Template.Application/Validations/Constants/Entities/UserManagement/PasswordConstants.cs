namespace Template.Application.Validations.Constants.Entities.UserManagement
{
    public static class PasswordConstants
    {
        // NOTE: Update the password regex depending on your need as well as the test methods
        public const string PasswordRegex = @"^(?=.*[A-Za-z])(?=.*\d).{10,}$";

        public const int MinLength = 10;
        public const int MaxLength = 100;
    }
}
