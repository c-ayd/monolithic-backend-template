using Cayd.AspNetCore.Settings.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Reflection;
using Template.Api.Configurations;
using Template.Application;
using Template.Infrastructure;
using Template.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegisterServices(builder.Configuration);

var app = builder.Build();

app.AddMiddlewares();

app.MapControllers();

app.Run();

public static partial class Program 
{
    public static void RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.ConfigureJwtBearer(configuration);
            });

        services.AddAuthorization();

        services.AddControllers();

        services.AddPersistenceServices(configuration);
        services.AddInfrastructureServices();
        services.AddApplicationServices();

        services.AddSettingsFromAssemblies(configuration,
            Assembly.GetAssembly(typeof(Program))!,
            Assembly.GetAssembly(typeof(Template.Persistence.ServiceRegistrations))!,
            Assembly.GetAssembly(typeof(Template.Infrastructure.ServiceRegistrations))!,
            Assembly.GetAssembly(typeof(Template.Application.ServiceRegistrations))!);

        services.AddLocalization(config => config.ResourcesPath = "Resources");
    }

    public static void AddMiddlewares(this IApplicationBuilder app)
    {
        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestLocalization(new RequestLocalizationOptions()
            .SetDefaultCulture("en")
            .AddSupportedUICultures(
                "en",
                "de"
            ));
    }
}
