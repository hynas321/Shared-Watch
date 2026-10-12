using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Google.Apis.Services;
using WebApi.Application.Options;
using WebApi.Application.Repositories;
using WebApi.Application.Services;
using WebApi.Infrastructure.Persistence;
using WebApi.Infrastructure.Persistence.Repositories;
using WebApi.Infrastructure.VideoStorage;
using WebApi.Infrastructure.Redis;
using WebApi.Infrastructure.YouTube;

namespace WebApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseConnection = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(databaseConnection))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(databaseConnection));
        services.Configure<VideoStorageOptions>(configuration.GetSection(VideoStorageOptions.SectionName));
        services.Configure<YouTubeApiOptions>(configuration.GetSection(YouTubeApiOptions.SectionName));
        services.AddSingleton<IJwtTokenService, Security.JwtTokenService>();
        services.AddSingleton<IYouTubeAPIService>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<YouTubeApiOptions>>().Value;
            return new YouTubeAPIService(new BaseClientService.Initializer { ApiKey = options.ApiKey });
        });
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IPlaylistRepository, PlaylistRepository>();
        services.AddSingleton<IUploadedVideoStorage, FileSystemUploadedVideoStorage>();
        services.AddSingleton<IHubConnectionRegistry, RedisHubConnectionRegistry>();
        services.AddSingleton<IVideoPlayerStateService, RedisVideoPlayerStateService>();
        services.AddSingleton<IRoomActivityTracker, RedisRoomActivityTracker>();
        return services;
    }
}


