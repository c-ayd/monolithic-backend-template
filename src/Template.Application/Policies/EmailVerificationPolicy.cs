namespace Template.Application.Policies
{
    public static class EmailVerificationPolicy
    {
        public const string PolicyName = "Email Verification Policy";
        public const string ClaimName = "EmailVerification";
        public const string ClaimValue = "True";
    }
}
