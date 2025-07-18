using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Template.Test.Utility.Hosting.Endpoints
{
    public static partial class TestEndpoints
    {
        public static void AddAuthenticationEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/test/authentication", () => Results.Ok())
                .RequireAuthorization();
        }
    }
}
