namespace Template.Application.Validations.Constants.Entities.UserManagement
{
    public static class UserConstants
    {
        // NOTE: Update the email regex depending on your need as well as the test methods
        public const string EmailRegex = @"^[a-zA-Z0-9][a-zA-Z0-9._-]*@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]\.[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]$";

        public const int EmailMaxLength = 256;
    }
}
