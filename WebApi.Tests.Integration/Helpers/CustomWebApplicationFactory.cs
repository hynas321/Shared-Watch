using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WebApi.Application.Services.Interfaces;
using WebApi.Api.SignalR.Interfaces;
using WebApi.Core.Entities.In_memory;
using WebApi.Application.HostedServices;
using System.Reflection;
using System.Collections.Concurrent;

public class MockYouTubeAPIService : IYouTubeAPIService
{
    public Task<int> GetVideoDurationAsync(string videoUrl)
    {
        return Task.FromResult(300);
    }

    public Task<string?> GetVideoTitleAsync(string videoUrl)
    {
        return Task.FromResult<string?>("Test Video Title");
    }

    public Task<string?> GetVideoThumbnailUrlAsync(string videoUrl)
    {
        return Task.FromResult<string?>("https://example.com/thumbnail.jpg"); // Return fake thumbnail URL
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var youtubeServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IYouTubeAPIService));

            if (youtubeServiceDescriptor != null)
            {
                services.Remove(youtubeServiceDescriptor);
            }

            services.AddSingleton<IYouTubeAPIService, MockYouTubeAPIService>();

            var allDescriptors = services.ToList();
            foreach (var descriptor in allDescriptors)
            {
                if (descriptor.ServiceType != null &&
                    descriptor.ServiceType.Name.Contains("DbContextFactory"))
                {
                    services.Remove(descriptor);
                }
            }

            var hostedServiceDescriptors = services.Where(
                d => d.ServiceType == typeof(IHostedService)).ToList();

            foreach (var descriptor in hostedServiceDescriptors)
            {
                if (descriptor.ImplementationType == typeof(DatabaseCleanup))
                {
                    services.Remove(descriptor);
                }
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
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

    public void ResetSingletonState()
    {
        try
        {
            var videoPlayerService = Services.GetService<IVideoPlayerStateService>();
            if (videoPlayerService != null)
            {
                ClearPrivateField<ConcurrentDictionary<string, object>>(videoPlayerService, "_roomLocks");
                ClearPrivateField<ConcurrentDictionary<string, VideoPlayer>>(videoPlayerService, "_roomStates");
            }

            var hubMapper = Services.GetService<IHubConnectionMapper>();
            if (hubMapper != null)
            {
                ClearAllConcurrentDictionaries(hubMapper);
            }
        }
        catch (Exception) {}
    }

    private void ClearPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var value = field.GetValue(target);
            if (value is ConcurrentDictionary<string, object> dict)
            {
                dict.Clear();
            }
            else if (value is ConcurrentDictionary<string, VideoPlayer> dict2)
            {
                dict2.Clear();
            }
        }
    }

    private void ClearAllConcurrentDictionaries(object target)
    {
        var fields = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var value = field.GetValue(target);
            if (value != null)
            {
                var clearMethod = value.GetType().GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                clearMethod?.Invoke(value, null);
            }
        }
    }

    public new async Task InitializeAsync()
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
