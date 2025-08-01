using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Template.Application.Policies;

namespace Template.Test.Utility.Hosting.Endpoints
{
    public static partial class TestEndpoints
    {
        public static void AddEmailVerifiactionPolicyEndpoint(this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/test/email-verification-policy", () => Results.Ok())
                .RequireAuthorization(EmailVerificationPolicy.PolicyName);
        }
    }
}
