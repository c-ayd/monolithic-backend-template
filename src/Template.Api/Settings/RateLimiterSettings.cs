using Cayd.AspNetCore.Settings;

namespace Template.Api.Settings
{
    public class RateLimiterSettings : ISettings
    {
        public static string SettingsKey => "RateLimiter";

        public required TokenBucketSettings TokenBucket { get; set; }


        public class TokenBucketSettings
        {
            public required string PolicyName { get; set; }
            public required int TokenLimit { get; set; }
            public required int QueueLimit { get; set; }
            public required int ReplenishmentPeriodInSeconds { get; set; }
            public required int TokenPerReplenishmentPeriod { get; set; }
        }
    }
}
