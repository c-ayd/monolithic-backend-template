using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Abstractions.Messaging.Templates;
using Template.Infrastructure.Messaging.Templates;

namespace Template.Test.Utility.Hosting
{
    public class TestHostStartup
    {
        private readonly IConfiguration _configuration;

        public TestHostStartup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IEmailTemplates, EmailTemplates>();

            services.AddLocalization(config => config.ResourcesPath = "Resources");
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestLocalization(new RequestLocalizationOptions()
                .SetDefaultCulture("en")
                .AddSupportedUICultures(
                    "en",
                    "de"
                ));

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/localization", async context =>
                {
                    var emailTemplates = context.RequestServices.GetRequiredService<IEmailTemplates>();
                    if (emailTemplates == null)
                    {
                        await context.Response.WriteAsync("Email Templates service is null");
                        return;
                    }

                    var template = emailTemplates.GetEmailVerificationTemplate("abc", 1);
                    if (template.Subject == null)
                    {
                        await context.Response.WriteAsync("The subject of the template is null");
                        return;
                    }

                    await context.Response.WriteAsync(template.Subject);
                });
            });
        }
    }
}
