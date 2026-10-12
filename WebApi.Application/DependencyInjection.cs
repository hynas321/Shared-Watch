using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using WebApi.Application.Options;
using WebApi.Application.Services;

namespace WebApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.Key), "Jwt:Key must be configured.")
            .Validate(options => Encoding.UTF8.GetByteCount(options.Key!) >= 32, "Jwt:Key must contain at least 32 UTF-8 bytes for HS256.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer must be configured.")
            .ValidateOnStart();
        services.AddOptions<ChatOptions>()
            .Bind(configuration.GetSection(ChatOptions.SectionName))
            .Validate(options => options.MaxMessages > 0, "Chat:MaxMessages must be greater than zero.")
            .Validate(options => options.MaxMessageTextLength > 0, "Chat:MaxMessageTextLength must be greater than zero.")
            .ValidateOnStart();
        services.Configure<GlobalAdminOptions>(options =>
            options.Token = configuration[GlobalAdminOptions.ConfigurationKey] ?? string.Empty);
        services.AddSingleton<IRoomPasswordService, Services.RoomPasswordService>();
        services.AddScoped<Services.RoomApplicationService>();
        services.AddScoped<IPlaylistApplicationService, Services.PlaylistApplicationService>();
        services.AddScoped<Services.VideoFileApplicationService>();
        services.AddScoped<IRoomLifecycleService, Services.RoomLifecycleService>();
        return services;
    }
}
