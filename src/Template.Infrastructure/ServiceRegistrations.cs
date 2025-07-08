using Microsoft.Extensions.DependencyInjection;
using Template.Application.Abstractions.Crypto;
using Template.Infrastructure.Crypto;

namespace Template.Infrastructure
{
    public static class ServiceRegistrations
    {
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<ITokenGenerator, TokenGenerator>();
        }
    }
}
