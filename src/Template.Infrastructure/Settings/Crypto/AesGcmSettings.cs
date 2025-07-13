using Cayd.AspNetCore.Settings;

namespace Template.Infrastructure.Settings.Crypto
{
    public class AesGcmSettings : ISettings
    {
        public static string SettingsKey => "Crypto:AesGcm";

        public required string Key { get; set; }
    }
}
