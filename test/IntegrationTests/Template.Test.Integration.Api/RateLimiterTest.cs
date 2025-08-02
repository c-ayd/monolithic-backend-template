using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Template.Api.Configurations;
using Template.Api.Settings;
using Template.Test.Utility;

namespace Template.Test.Integration.Api
{
    public class RateLimiterTest
    {
        private const string _tokenBucketEndpoint = "/test/rate-limiter/token-bucket";

        private readonly IHost _host;
        private readonly HttpClient _client;

        private readonly IConfiguration _configuration;
        private readonly RateLimiterSettings _rateLimiterSettings;

        public RateLimiterTest()
        {
            _configuration = ConfigurationHelper.CreateConfiguration();
            _rateLimiterSettings = _configuration.GetSection(RateLimiterSettings.SettingsKey).Get<RateLimiterSettings>()!;

            _host = new HostBuilder()
                .ConfigureWebHost(hostBuilder =>
                {
                    hostBuilder.UseTestServer()
                        .UseConfiguration(_configuration)
                        .ConfigureServices((context, services) =>
                        {
                            services.AddRouting();
                            services.ConfigureRateLimiter(context.Configuration);
                        })
                        .Configure(appBuilder =>
                        {
                            appBuilder.UseRouting();
                            appBuilder.UseRateLimiter();
                            appBuilder.UseEndpoints(endpoints =>
                            {
                                AddEndpoints(endpoints);
                            });
                        });
                })
                .Build();

            _host.Start();

            _client = _host.GetTestClient();
        }

        private void AddEndpoints(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(_tokenBucketEndpoint, () => Results.Ok())
                .RequireRateLimiting(_rateLimiterSettings.TokenBucket.PolicyName);
        }

        [Fact]
        public async Task TokenBucket_WhenSentRequestsAreWithinLimit_ShouldReturnOk_And_WhenSentRequestsExceedLimit_ShouldReturnTooManyRequests()
        {
            // Act
            var tasks = new List<Task<HttpResponseMessage>>();

            /** 1 - Within limit */
            var start = DateTime.UtcNow;
            for (int i = 0; i < _rateLimiterSettings.TokenBucket.TokenLimit; ++i)
            {
                tasks.Add(_client.GetAsync(_tokenBucketEndpoint));
            }

            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                Assert.Equal(HttpStatusCode.OK, task.Result.StatusCode);
            }
            var end = DateTime.UtcNow;

            /** 1 - Wait until tokens are replenished */
            var difference = (end - start).TotalSeconds;
            if (difference < _rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds)
            {
                var delay = _rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds - difference + (_rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds / 2.0);
                await Task.Delay((int)(delay * 1000));
            }

            /** 1 - Test again */
            tasks.Clear();
            start = DateTime.UtcNow;
            for (int i = 0; i < _rateLimiterSettings.TokenBucket.TokenPerReplenishmentPeriod; ++i)
            {
                tasks.Add(_client.GetAsync(_tokenBucketEndpoint));
            }

            await Task.WhenAll(tasks);
            foreach (var task in tasks)
            {
                Assert.Equal(HttpStatusCode.OK, task.Result.StatusCode);
            }
            end = DateTime.UtcNow;

            /** 2 - Wait until tokens are replenished for the second test */
            difference = (end - start).TotalSeconds;
            if (difference < _rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds)
            {
                var delay = _rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds - difference + (_rateLimiterSettings.TokenBucket.ReplenishmentPeriodInSeconds / 2.0);
                await Task.Delay((int)(delay * 1000));
            }

            /** 2 - Exceeds limit */
            tasks.Clear();
            for (int i = 0; i < _rateLimiterSettings.TokenBucket.TokenLimit + 1; ++i)
            {
                tasks.Add(_client.GetAsync(_tokenBucketEndpoint));
            }

            await Task.WhenAll(tasks);
            Assert.True(tasks.Any(t => t.Result.StatusCode == HttpStatusCode.TooManyRequests), "All requests passed the rate limiter.");
        }
    }
}
