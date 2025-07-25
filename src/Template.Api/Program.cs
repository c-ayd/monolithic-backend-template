using Cayd.AspNetCore.FlexLog.DependencyInjection;
using Cayd.AspNetCore.Settings.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Template.Api;
using Template.Api.Configurations;
using Template.Api.Filters;
using Template.Api.Logging.Sinks;
using Template.Api.Middlewares;
using Template.Application;
using Template.Infrastructure;
using Template.Persistence;
using Template.Persistence.SeedData;
using Template.Persistence.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServices(builder.Configuration);

builder.Services.AddFlexLog(builder.Configuration, config =>
{
    var connStrings = builder.Configuration.GetSection(ConnectionStringsSettings.SettingsKey).Get<ConnectionStringsSettings>()!;

    config.AddSink(new DatabaseSink(connStrings.Log))
        .AddFallbackSink(new FileSink());
});

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

        services.AddControllers(config =>
        {
            config.Filters.Add(new ProblemDetailsPopulaterFilter());
        }).AddApplicationPart(typeof(Program).Assembly);

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.ConfigureInvalidModelStateResponse();
        });

        services.AddPresentationServices();
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

        app.UseFlexLog();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<RequestContextPopulator>();

        app.UseRequestLocalization(new RequestLocalizationOptions()
            .SetDefaultCulture("en")
            .AddSupportedUICultures(
                "en",
                "de"
            ));
    }
}
