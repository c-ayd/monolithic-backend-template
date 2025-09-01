using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using Template.Api.Utilities;
using Template.Infrastructure.Settings.Authentication;

namespace Template.Api.Configurations
{
    public static partial class Configurations
    {
        private static readonly string JwtEventResponseHasStartedKey = "JwtEventResponseHasStarted";

        public static void ConfigureJwtBearer(this JwtBearerOptions options, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(JwtSettings.SettingsKey).Get<JwtSettings>()!;

            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                RequireSignedTokens = true,

                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidAlgorithms = new List<string>() { SecurityAlgorithms.HmacSha256 },
                ClockSkew = TimeSpan.Zero,
                LifetimeValidator = delegate(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
                {
                    if (notBefore != null && notBefore > DateTime.UtcNow)
                        return false;

                    return expires != null && expires > DateTime.UtcNow;
                },

                RoleClaimType = ClaimTypes.Role
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
            var responseHasStarted = (bool?)context.HttpContext.Items[JwtEventResponseHasStartedKey];
            if (responseHasStarted.HasValue && responseHasStarted.Value)
                return;

            context.HandleResponse();
            await JwtEventFailed(context.HttpContext, StatusCodes.Status401Unauthorized);
        }

        private static async Task JwtAuthenticationFailed(AuthenticationFailedContext context)
        {
            var responseHasStarted = (bool?)context.HttpContext.Items[JwtEventResponseHasStartedKey];
            if (responseHasStarted.HasValue && responseHasStarted.Value)
                return;

            if (context.HttpContext.GetEndpoint()?.Metadata?.OfType<AuthorizeAttribute>().Any() ?? false)
            {
                await JwtEventFailed(context.HttpContext, StatusCodes.Status401Unauthorized);
            }
        }

        private static async Task JwtForbidden(ForbiddenContext context)
        {
            var responseHasStarted = (bool?)context.HttpContext.Items[JwtEventResponseHasStartedKey];
            if (responseHasStarted.HasValue && responseHasStarted.Value)
                return;

            await JwtEventFailed(context.HttpContext, StatusCodes.Status403Forbidden);
        }

        private static async Task JwtEventFailed(HttpContext context, int statusCode)
        {
            if (context.Response.HasStarted)
                return;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsJsonAsync(JsonUtility.Fail(statusCode).Value);

            context.Items[JwtEventResponseHasStartedKey] = true;
        }
    }
}
