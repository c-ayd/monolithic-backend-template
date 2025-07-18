using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mime;
using System.Text;
using Template.Api.Utilities;
using Template.Infrastructure.Settings.Authentication;

namespace Template.Api.Configurations
{
    public static partial class Configurations
    {
        public static void ConfigureJwtBearer(this JwtBearerOptions options, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                LifetimeValidator = delegate(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
                {
                    if (notBefore != null && notBefore > DateTime.UtcNow)
                        return false;

                    return expires != null && expires > DateTime.UtcNow;
                }
            };

            options.Events = new JwtBearerEvents()
            {
                OnChallenge = JwtChallenge,
                OnAuthenticationFailed = JwtAuthenticationFailed,
                OnForbidden = JwtForbidden,
            };
        }

        private static async Task JwtChallenge(JwtBearerChallengeContext context)
        {
            context.HandleResponse();

            await JwtEventFailed(context.Response, StatusCodes.Status401Unauthorized);
        }

        private static async Task JwtAuthenticationFailed(AuthenticationFailedContext context)
        {
            if (context.HttpContext.GetEndpoint()?.Metadata?.OfType<AuthorizeAttribute>().Any() ?? false)
            {
                await JwtEventFailed(context.Response, StatusCodes.Status401Unauthorized);
            }
        }

        private static async Task JwtForbidden(ForbiddenContext context)
        {
            await JwtEventFailed(context.Response, StatusCodes.Status403Forbidden);
        }

        private static async Task JwtEventFailed(HttpResponse response, int statusCode)
        {
            if (!response.HasStarted)
            {
                response.StatusCode = statusCode;
                response.ContentType = MediaTypeNames.Application.Json;
                await response.WriteAsJsonAsync(JsonUtility.Fail(statusCode));
            }
        }
    }
}
