using Cayd.AspNetCore.Settings;

namespace Template.Infrastructure.Settings.Messaging.Templates
{
    public class TemplateLinksSettings : ISettings
    {
        public static string SettingsKey => "TemplateLinks";

        public required string EmailVerification { get; set; }
        public required string ResetPassword { get; set; }
    }
}
