using Cayd.AspNetCore.FlexLog.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Template.Persistence.DbContexts;
using Template.Test.Utility.Hosting.Endpoints;
using Template.Test.Utility.Hosting.Policies;
using Template.Test.Utility.Hosting.Sinks;

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
                            services.AddServices(context.Configuration);
                            services.AddAuthorization(config =>
                            {
                                config.AddPolicy(TestPolicy.PolicyName, p =>
                                {
                                    p.RequireClaim(TestPolicy.ClaimName, "test-value");
                                });
                            });
                            services.AddFlexLog(context.Configuration, config =>
                            {
                                config.AddSink(new TestSink());
                            });
                        })
                        .Configure(appBuilder =>
                        {
                            appBuilder.UseRouting();
                            appBuilder.AddMiddlewares();
                            appBuilder.UseEndpoints(endpoints =>
                            {
                                AddAllTestEndpoints(endpoints);
                                endpoints.MapControllers();
                            });
                        });
                })
                .Build();

            await Host.StartAsync();
            Client = Host.GetTestClient();

            await CreateDatabase();
            SetDefaultOptions();
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

        private async Task CreateDatabase()
        {
            var dbContext = Host.Services.GetRequiredService<AppDbContext>();

            if (await dbContext.Database.CanConnectAsync())
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            await dbContext.Database.MigrateAsync();
        }

        private void SetDefaultOptions()
        {
            ResetAcceptLanguage();
            RemoveJwtBearerToken();
            SetEmailSenderResult(true);
        }
    }
}
