using Cayd.AspNetCore.Settings;

namespace Template.Infrastructure.Settings.Authentication
{
    public class JwtSettings : ISettings
    {
        public static string SettingsKey => "Jwt";

        public required string SecretKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public required int AccessTokenLifeSpanInMinutes { get; set; }
        public required int RefreshTokenLifeSpanInDays { get; set; }
    }
}
