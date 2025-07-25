using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Configurations
{
    public static partial class Configurations
    {
        public static void ConfigureInvalidModelStateResponse(this ApiBehaviorOptions options)
        {
            options.InvalidModelStateResponseFactory = (context) =>
            {
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid model",
                    Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}",
                    Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.1",
                    Extensions = new Dictionary<string, object?>()
                    {
                        { "traceId", context.HttpContext.TraceIdentifier }
                    }
                };

                return new ObjectResult(problemDetails)
                {
                    StatusCode = problemDetails.Status
                };
            };
        }
    }
}
