using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Template.Application.Abstractions.UOW;
using Template.Persistence.DbContexts;
using Template.Persistence.Settings;
using Template.Persistence.UOW;

namespace Template.Persistence
{
    public static class ServiceRegistrations
    {
        public static void AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connStrings = configuration.GetSection(ConnectionStringsSettings.SettingsKey).Get<ConnectionStringsSettings>()!;
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connStrings.App));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
