using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Template.Application.Validations;

namespace Template.Application
{
    public static class ServiceRegistrations
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            var currentAssembly = Assembly.GetExecutingAssembly()!;

            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(currentAssembly);
                config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(currentAssembly);
        }
    }
}
