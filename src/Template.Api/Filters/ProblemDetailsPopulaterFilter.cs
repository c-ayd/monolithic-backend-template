using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Template.Api.Filters
{
    public class ProblemDetailsPopulaterFilter : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult &&
                objectResult.Value is ProblemDetails problemDetails)
            {
                problemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                problemDetails.Extensions = new Dictionary<string, object?>()
                {
                    { "traceId", context.HttpContext.TraceIdentifier }
                };
            }
        }
    }
}
