using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Application.Constants;
using WebApi.Application.Options;

namespace WebApi.Extensions;

public static class AuthenticationServiceExtensions
{
    public static IServiceCollection AddSharedWatchAuthentication(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
            {
                ConfigureJwt(options, jwtOptionsAccessor.Value, environment);
            });

        return services;
    }

    private static void ConfigureJwt(
        JwtBearerOptions options,
        JwtOptions jwtOptions,
        IWebHostEnvironment environment)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.Key))
        {
            throw new InvalidOperationException(
                "Jwt:Key must be configured through user secrets or the environment.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key))
        };

        options.RequireHttpsMetadata = environment.IsProduction();
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                context.Token = !string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments("/Hub/Room")
                    ? accessToken
                    : context.Request.Cookies[AuthenticationConstants.AccessTokenCookie];

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuthentication");

                logger.LogWarning(
                    context.Exception,
                    "JWT authentication failed for {Method} {Path}. TraceId: {TraceId}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    context.HttpContext.TraceIdentifier);

                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtAuthentication");

                logger.LogWarning(
                    "JWT challenge for {Method} {Path}. Error: {Error}, Description: {Description}, TraceId: {TraceId}",
                    context.HttpContext.Request.Method,
                    context.HttpContext.Request.Path,
                    context.Error,
                    context.ErrorDescription,
                    context.HttpContext.TraceIdentifier);

                return Task.CompletedTask;
            }
        };
    }
}
