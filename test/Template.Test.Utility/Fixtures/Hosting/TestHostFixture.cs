using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Template.Test.Utility.Hosting;

namespace Template.Test.Utility.Fixtures.Hosting
{
    public class TestHostFixture : IAsyncLifetime
    {
        public IHost TestHost { get; private set; } = null!;
        public HttpClient TestClient { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            var config = ConfigurationHelper.CreateConfiguration();
            TestHost = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, configBuilder) =>
                {
                    configBuilder.AddConfiguration(config);
                })
                .ConfigureWebHostDefaults(builder =>
                {
                    builder.UseTestServer()
                        .UseStartup<TestHostStartup>();
                })
                .Build();

            await TestHost.StartAsync();
            TestClient = TestHost.GetTestClient();
        }

        public async Task DisposeAsync()
        {
            await TestHost.StopAsync();
            TestHost.Dispose();
            TestClient.Dispose();
        }
    }
}
