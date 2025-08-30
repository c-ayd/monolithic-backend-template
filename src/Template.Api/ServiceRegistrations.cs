using Template.Api.Http;
using Template.Application.Abstractions.Http;

namespace Template.Api
{
    public static class ServiceRegistrations
    {
        public static void AddApiServices(this IServiceCollection services)
        {
            services.AddScoped<IRequestContext, RequestContext>();
        }
    }
}
