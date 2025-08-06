using Cayd.AspNetCore.Settings;

namespace Template.Api.Settings
{
    public class CorsSettings : ISettings
    {
        /**
         * NOTE: This is an example of CORS options. If you have more specific CORS options,
         * you have to edit the related part in appsettings.json as well as this file to match
         * both structures. Afterwards, you also have to change how CORS options are configured
         * and which CORS policies are added in Program.cs.
         */

        public static string SettingsKey => "Cors:DefaultPolicy";

        public required List<string> Origins { get; set; }
    }
}
