using Microsoft.Extensions.DependencyInjection;
using Template.Application.Abstractions.Authentication;
using Template.Application.Abstractions.Crypto;
using Template.Application.Abstractions.Messaging;
using Template.Application.Abstractions.Messaging.Templates;
using Template.Infrastructure.Authentication;
using Template.Infrastructure.Crypto;
using Template.Infrastructure.Messaging;
using Template.Infrastructure.Messaging.Templates;

namespace Template.Infrastructure
{
    public static class ServiceRegistrations
    {
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<IJwt, Jwt>();

            services.AddSingleton<ITokenGenerator, TokenGenerator>();
            services.AddSingleton<IHashing, Hashing>();
            services.AddSingleton<IEncryption, AesGcmEncryption>();

            services.AddSingleton<IEmailSender, Smtp>();
            services.AddSingleton<IEmailTemplates, EmailTemplates>();
        }
    }
}
