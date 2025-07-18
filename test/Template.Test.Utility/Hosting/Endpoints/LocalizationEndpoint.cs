using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Template.Application.Abstractions.Messaging.Templates;

namespace Template.Test.Utility.Hosting.Endpoints
{
    public static partial class TestEndpoints
    {
        public static void AddLocalizationEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/test/localization", (IEmailTemplates emailTemplates) =>
            {
                var template = emailTemplates.GetEmailVerificationTemplate("abc", 1);
                if (template.Subject == null)
                    return Results.NotFound("The subject of the template is null.");

                return Results.Ok(template.Subject);
            });
        }
    }
}
