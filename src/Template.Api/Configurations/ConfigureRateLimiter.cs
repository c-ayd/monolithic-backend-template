using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;
using System.Threading.RateLimiting;
using Template.Api.Settings;
using Template.Api.Utilities;

namespace Template.Api.Configurations
{
    public static partial class Configurations
    {
        public static void ConfigureRateLimiter(this IServiceCollection services, IConfiguration configuration)
        {
            var rateLimiterSettings = configuration.GetSection(RateLimiterSettings.SettingsKey).Get<RateLimiterSettings>()!;

            services.AddRateLimiter(config =>
            {
                config.AddTokenBucketLimiter(rateLimiterSettings.TokenBucket.PolicyName, options =>
                {
                    options.TokenLimit = rateLimiterSettings.TokenBucket.TokenLimit;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    options.QueueLimit = rateLimiterSettings.TokenBucket.QueueLimit;
                    options.ReplenishmentPeriod = TimeSpan.FromSeconds(rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds);
                    options.TokensPerPeriod = rateLimiterSettings.TokenBucket.TokenPerReplenishmentPeriod;
                    options.AutoReplenishment = true;
                });

                config.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                config.OnRejected = async (context, cancellationToken) =>
                {
                    var statusCode = StatusCodes.Status429TooManyRequests;

                    context.HttpContext.Response.StatusCode = statusCode;
                    context.HttpContext.Response.ContentType = MediaTypeNames.Application.Json;
                    await context.HttpContext.Response.WriteAsJsonAsync(JsonUtility.Fail(statusCode, new
                    {
                        WaitForSeconds = rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds
                    }));
                };
            });
        }
    }
}
