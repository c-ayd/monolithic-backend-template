using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Template.Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService)
        {
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext()
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal server error",
                    Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
                    Type = "https://datatracker.ietf.org/doc/html/rfc9110#section-15.6.1",
                    Extensions = new Dictionary<string, object?>()
                    {
                        { "traceId", httpContext.TraceIdentifier }
                    }
                },
                Exception = exception
            });
        }
    }
}
