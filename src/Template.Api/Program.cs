using Cayd.AspNetCore.Settings.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Template.Api.Configurations;
using Template.Api.Middlewares;
using Template.Application;
using Template.Infrastructure;
using Template.Persistence;
using Template.Persistence.SeedData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration);

var app = builder.Build();

//await app.SeedDataAppDbContext();

app.AddMiddlewares();

app.MapControllers();

app.Run();

public static partial class Program 
{
    public static void AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.ConfigureJwtBearer(configuration);
            });

        services.AddAuthorization();

        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.ConfigureInvalidModelStateResponse();
        });

        services.AddPersistenceServices(configuration);
        services.AddInfrastructureServices();
        services.AddApplicationServices();

        services.AddSettingsFromAssemblies(configuration,
            Assembly.GetAssembly(typeof(Program))!,
            Assembly.GetAssembly(typeof(Template.Persistence.ServiceRegistrations))!,
            Assembly.GetAssembly(typeof(Template.Infrastructure.ServiceRegistrations))!,
            Assembly.GetAssembly(typeof(Template.Application.ServiceRegistrations))!);

        services.AddLocalization(config => config.ResourcesPath = "Resources");

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
    }

    public static void AddMiddlewares(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();

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
