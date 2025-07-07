using Cayd.AspNetCore.Settings;

namespace Template.Persistence.Settings
{
    public class ConnectionStringsSettings : ISettings
    {
        public static string SettingsKey => "ConnectionStrings";

        public required string App { get; set; }
        public string? Test { get; set; }
    }
}
