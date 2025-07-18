using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Template.Test.Utility.Hosting.Endpoints;
using Template.Test.Utility.Hosting.Policies;

namespace Template.Test.Utility.Fixtures.Hosting
{
    public partial class TestHostFixture : IAsyncLifetime
    {
        public IHost Host { get; private set; } = null!;
        public HttpClient Client { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Host = new HostBuilder()
                .ConfigureWebHost(hostBuilder =>
                {
                    hostBuilder.UseTestServer()
                        .UseConfiguration(ConfigurationHelper.CreateConfiguration())
                        .ConfigureServices((context, services) =>
                        {
                            services.RegisterServices(context.Configuration);
                            services.AddAuthorization(config =>
                            {
                                config.AddPolicy(TestPolicy.PolicyName, p =>
                                {
                                    p.RequireClaim(TestPolicy.ClaimName, "test-value");
                                });
                            });
                        })
                        .Configure(appBuilder =>
                        {
                            appBuilder.UseRouting();
                            appBuilder.AddMiddlewares();
                            appBuilder.UseEndpoints(endpoints =>
                            {
                                AddAllTestEndpoints(endpoints);
                            });
                        });
                })
                .Build();

            await Host.StartAsync();
            Client = Host.GetTestClient();
        }

        public async Task DisposeAsync()
        {
            await Host.StopAsync();
            Host.Dispose();
            Client.Dispose();
        }

        private void AddAllTestEndpoints(IEndpointRouteBuilder endpoints)
        {
            var methods = typeof(TestEndpoints).GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var method in methods)
            {
                method.Invoke(null, new object[] { endpoints });
            }
        }
    }
}
