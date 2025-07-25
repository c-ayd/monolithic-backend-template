using Cayd.AspNetCore.Settings;

namespace Template.Application.Settings
{
    public class AccountLockSettings : ISettings
    {
        public static string SettingsKey => "AccountLock";

        public required int FailedAttemptsForFirstLock { get; set; }
        public required int FailedAttemptsForSecondLock { get; set; }
        public required int FirstLockTimeInMinutes { get; set; }
        public required int SecondLockTimeInMinutes { get; set; }
    }
}
