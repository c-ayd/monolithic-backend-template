using Cayd.AspNetCore.Settings;

namespace Template.Persistence.Settings
{
    public class SeedDataAppDbContextSettings : ISettings
    {
        /**
         * NOTE: This class is used to read seed data from the configuration. Depending on specific needs,
         * the structure can be changed.
         */

        public static string SettingsKey => "SeedData:AppDbContext";

        public required List<string> AdminEmails { get; set; }
    }
}
