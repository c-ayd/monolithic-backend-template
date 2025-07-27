namespace Template.Api.Utilities
{
    public static class CookieUtility
    {
        public const string RefreshTokenKey = "RefreshToken";

        public static void AddRefreshToken(this IResponseCookies cookies, string refreshToken, DateTime expirationDate)
        {
            var options = new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                Path = "/auth",
                SameSite = SameSiteMode.None,
                Expires = expirationDate,
                // Domain = NOTE: Set domain if needed
            };

            cookies.Append(RefreshTokenKey, refreshToken, options);
        }

        public static void RemoveRefreshToken(this IResponseCookies cookies)
        {
            cookies.Delete(RefreshTokenKey);
        }
    }
}
