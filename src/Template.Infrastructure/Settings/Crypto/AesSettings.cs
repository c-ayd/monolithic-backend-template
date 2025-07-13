using Cayd.AspNetCore.Settings;

namespace Template.Infrastructure.Settings.Crypto
{
    public class AesSettings : ISettings
    {
        public static string SettingsKey => "Crypto:Aes";

        public required string Key { get; set; }
    }
}
