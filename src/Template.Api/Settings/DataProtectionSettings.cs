using Cayd.AspNetCore.Settings;

namespace Template.Api.Settings
{
    public class DataProtectionSettings : ISettings
    {
        public static string SettingsKey => "DataProtection";

        public required string FilePath { get; set; }
    }
}
