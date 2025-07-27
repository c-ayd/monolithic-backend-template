using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using Template.Api.Http;
using Template.Application.Abstractions.Http;
using Template.Application.Validations.Constants.Entities.UserManagement;

namespace Template.Api.Middlewares
{
    public class RequestContextPopulator
    {
        private readonly RequestDelegate _next;

        public RequestContextPopulator(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestContext = (RequestContext)context.RequestServices.GetRequiredService<IRequestContext>();
            if (requestContext != null)
            {
                requestContext.UserId = GetUserId(context.User);
                requestContext.IpAddress = context.Connection.RemoteIpAddress;
                requestContext.DeviceInfo = GetDeviceInfo(context.Request.Headers.UserAgent);
                requestContext.PreferredLanguages = GetPreferredLanguages(context.Request.Headers.AcceptLanguage);
            }

            await _next(context);
        }

        private Guid? GetUserId(ClaimsPrincipal user)
        {
            var nameIdentifier = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (nameIdentifier == null)
                return null;

            return Guid.TryParse(nameIdentifier, out var userId) ? userId : null;
        }

        private string? GetDeviceInfo(StringValues userAgentHeader)
        {
            if (StringValues.IsNullOrEmpty(userAgentHeader))
                return null;

            var userAgentString = userAgentHeader.ToString();
            return userAgentString.Length <= LoginConstants.DeviceInfoMaxLength ?
                userAgentString : userAgentString.Substring(0, LoginConstants.DeviceInfoMaxLength);
        }

        private List<string> GetPreferredLanguages(StringValues acceptLanguageHeader)
        {
            if (StringValues.IsNullOrEmpty(acceptLanguageHeader))
                return IRequestContext.DefaultPreferredLanguages;

            return acceptLanguageHeader.ToString()
                .Split(',')
                .Select(s =>
                {
                    if (s.Contains(';'))
                    {
                        var parts = s.Split(';');
                        if (parts == null || parts.Length != 2)
                            return new { Language = s, Weight = 0.0 };

                        var qualityParts = parts[1].Split('=');
                        if (qualityParts.Length != 2)
                            return new { Language = parts[0], Weight = 0.0 };

                        if (double.TryParse(qualityParts[1], out double weight))
                            return new { Language = parts[0], Weight = weight };

                        return new { Language = parts[0], Weight = 0.0 };
                    }

                    return new { Language = s, Weight = 1.0 };
                })
                .GroupBy(l => l.Language)
                .Select(g => g.OrderByDescending(_ => _.Weight).First())
                .OrderByDescending(l => l.Weight)
                .Select(l => l.Language)
                .ToList();
        }
    }
}
