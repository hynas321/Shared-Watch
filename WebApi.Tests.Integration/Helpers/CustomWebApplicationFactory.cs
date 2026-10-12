using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using WebApi.Application.Services;
using WebApi.Core.Entities.In_memory;
using WebApi.Infrastructure.Persistence;
using WebApi.HostedServices;

public class MockYouTubeAPIService : IYouTubeAPIService
{
    private static bool IsInvalid(string videoUrl) => videoUrl.Contains("invalid-url", StringComparison.OrdinalIgnoreCase);

    public Task<int> GetVideoDurationAsync(string videoUrl) => Task.FromResult(IsInvalid(videoUrl) ? 0 : 300);

    public Task<int> GetVideoDurationAsync(string videoUrl, CancellationToken cancellationToken)
    {
        return Task.FromResult(IsInvalid(videoUrl) ? 0 : 300);
    }

    public Task<string?> GetVideoTitleAsync(string videoUrl)
    {
        return Task.FromResult<string?>(IsInvalid(videoUrl) ? null : "Test Video Title");
    }

    public Task<string?> GetVideoThumbnailUrlAsync(string videoUrl)
    {
        return Task.FromResult<string?>("https://example.com/thumbnail.jpg");
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var youtubeServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IYouTubeAPIService));

            if (youtubeServiceDescriptor != null)
            {
                services.Remove(youtubeServiceDescriptor);
            }

            services.AddSingleton<IYouTubeAPIService, MockYouTubeAPIService>();

            services.RemoveAll<IDbContextFactory<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            var contextOptionsConfigurations = services.Where(
                descriptor => descriptor.ServiceType.IsGenericType &&
                              descriptor.ServiceType.Name.Contains("DbContextOptionsConfiguration", StringComparison.Ordinal) &&
                              descriptor.ServiceType.GenericTypeArguments[0] == typeof(AppDbContext))
                .ToList();
            foreach (var descriptor in contextOptionsConfigurations)
            {
                services.Remove(descriptor);
            }

            var hostedServiceDescriptors = services.Where(
                d => d.ServiceType == typeof(IHostedService)).ToList();

            foreach (var descriptor in hostedServiceDescriptors)
            {
                if (descriptor.ImplementationType == typeof(RoomPersistenceCleanupService))
                {
                    services.Remove(descriptor);
                }
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });
        });
    }

    public AppDbContext GetDbContext()
    {
        var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public void ClearDatabase()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.ChatMessages.RemoveRange(db.ChatMessages.ToList());
        db.PlaylistVideos.RemoveRange(db.PlaylistVideos.ToList());

        var users = db.Users.ToList();
        if (users.Any())
        {
            db.Users.RemoveRange(users);
        }

        var rooms = db.Rooms.ToList();
        if (rooms.Any())
        {
            db.Rooms.RemoveRange(rooms);
        }

        db.SaveChanges();

        foreach (var entry in db.ChangeTracker.Entries().ToList())
        {
            entry.State = EntityState.Detached;
        }
    }

    public T GetRequiredService<T>() where T : notnull
    {
        using var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    public void ResetSingletonState() { }

    public async Task InitializeAsync()
    {
        ResetSingletonState();
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        ResetSingletonState();
        await base.DisposeAsync();
    }
}
