using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using WebApi.Api.Services;
using WebApi.Application;
using WebApi.Application.Options;
using WebApi.Application.Services;
using WebApi.Filters;
using WebApi.HostedServices;
using WebApi.Infrastructure;
using WebApi.SignalR;

namespace WebApi.Extensions;

public static class WebApiServiceExtensions
{
    public static IServiceCollection AddSharedWatchServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var redisConnection = RequireConnectionString(configuration, "Redis");
        RequireConnectionString(configuration, "DefaultConnection");

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));
        services.AddSwagger();
        services.AddSharedWatchAuthentication(environment);
        services.AddHttpContextAccessor();
        services.AddInfrastructure(configuration);
        services.AddApplication(configuration);
        services.AddVideoServices(environment);
        services.AddCorsPolicy(configuration);
        services.AddControllersWithSharedWatchJson();
        services.AddSignalRWithSharedWatchFilters(redisConnection);

        return services;
    }

    private static string RequireConnectionString(IConfiguration configuration, string name)
    {
        var connectionString = configuration.GetConnectionString(name);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"ConnectionStrings:{name} must be configured.");
        }

        return connectionString;
    }

    private static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' followed by a space and then your token."
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    private static IServiceCollection AddVideoServices(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.PostConfigure<VideoStorageOptions>(options =>
        {
            if (environment.IsProduction() && !Path.IsPathRooted(options.Path))
            {
                throw new InvalidOperationException(
                    "Production VideoStorage:Path must be an absolute path to shared persistent storage mounted on every API instance.");
            }
        });

        services.AddSingleton<VideoPlayerService>();
        services.AddSingleton<IVideoPlayerService>(sp => sp.GetRequiredService<VideoPlayerService>());
        services.AddHostedService<VideoPlayerService>(sp => sp.GetRequiredService<VideoPlayerService>());
        services.AddHostedService<RoomPersistenceCleanupService>();
        services.AddScoped<IHttpContextService, HttpContextService>();
        return services;
    }

    private static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApplication", policy =>
                policy.WithOrigins(configuration["Frontend:Origin"] ?? "http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }

    private static IServiceCollection AddControllersWithSharedWatchJson(
        this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });

        return services;
    }

    private static IServiceCollection AddSignalRWithSharedWatchFilters(
        this IServiceCollection services,
        string redisConnection)
    {
        services.AddSignalR(options =>
        {
            options.AddFilter<RoomHashAuthorizationFilter>();
            options.AddFilter<HubExceptionFilter>();
            options.KeepAliveInterval = TimeSpan.FromSeconds(10);
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(20);
            options.EnableDetailedErrors = false;
        }).AddStackExchangeRedis(redisConnection, options =>
        {
            options.Configuration.ChannelPrefix = RedisChannel.Literal("shared-watch");
        }).AddJsonProtocol(options =>
        {
            options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PayloadSerializerOptions.WriteIndented = true;
        });

        return services;
    }
}
