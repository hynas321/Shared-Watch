using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Constants;
using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;
using WebApi.Tests.Integration.Helpers;
using Xunit;

namespace WebApi.Tests.Integration.SignalR;

[Collection("Integration Tests")]
public class AppHubPlaylistTests : BaseIntegrationTest, IAsyncLifetime
{
    public AppHubPlaylistTests(IntegrationTestFixture fixture) : base(fixture) {}

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await CleanupConnectionsAsync();
        await Task.Delay(200);
    }

    private HubConnection _connection = null!;
    private string _roomHash = null!;

    private async Task SetupRoomAsync()
    {
        var db = Factory.GetDbContext();
        var room = new Room("TestRoom", "");
        room.UserPermissions.CanAddVideo = true;
        room.UserPermissions.CanRemoveVideo = true;
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        _roomHash = room.Hash!;

        var jwtService = Factory.GetRequiredService<IJwtTokenService>();
        var token = jwtService.GenerateToken("AdminUser", Role.Admin, _roomHash);

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/Hub/Room", options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Headers.Add("Authorization", $"Bearer {token}");
            })
            .Build();
    }

    private async Task CleanupConnectionsAsync()
    {
        if (_connection != null)
        {
            try
            {
                if (_connection.State == HubConnectionState.Connected)
                {
                    await _connection.StopAsync().ConfigureAwait(false);
                }
            }
            catch { }

            try
            {
                await _connection.DisposeAsync().ConfigureAwait(false);
            }
            catch { }
        }
    }

    [Fact]
    public async Task AddPlaylistVideo_ValidYouTubeUrl_ShouldAddAndBroadcast()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        var playlistVideo = new PlaylistVideo
        {
            Url = "https://www.youtube.com/watch?v=dQw4w9WgXcQ"
        };

        await _connection.InvokeAsync(HubMessages.AddPlaylistVideo, _roomHash, playlistVideo);
        await Task.Delay(500);

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.PlaylistVideos).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room.Should().NotBeNull();
        room!.PlaylistVideos.Should().HaveCount(1);

        await _connection.StopAsync();
    }

    [Fact]
    public async Task AddPlaylistVideo_MaxVideos_ShouldNotAdd()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.PlaylistVideos).FirstOrDefaultAsync(r => r.Hash == _roomHash);

        for (int i = 0; i < 10; i++)
        {
            room!.PlaylistVideos.Add(new PlaylistVideo
            {
                Hash = Guid.NewGuid().ToString(),
                Url = "https://www.youtube.com/watch?v=test" + i,
                Title = "Test Video " + i
            });
        }
        await db.SaveChangesAsync();

        await _connection.StartAsync();

        var playlistVideo = new PlaylistVideo
        {
            Url = "https://www.youtube.com/watch?v=testvideo"
        };

        PlaylistVideo? receivedResult = null;
        _connection.On(HubMessages.OnAddPlaylistVideo, (PlaylistVideo? video) =>
        {
            receivedResult = video;
        });

        await _connection.InvokeAsync(HubMessages.AddPlaylistVideo, _roomHash, playlistVideo);
        await Task.Delay(100);

        receivedResult.Should().BeNull();
        var updatedRoom = await db.Rooms.Include(r => r.PlaylistVideos).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        updatedRoom!.PlaylistVideos.Should().HaveCount(10);

        await _connection.StopAsync();
    }

    [Fact]
    public async Task AddPlaylistVideo_InvalidUrl_ShouldNotAdd()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        var playlistVideo = new PlaylistVideo
        {
            Url = "https://invalid-url.com/video"
        };

        await _connection.InvokeAsync(HubMessages.AddPlaylistVideo, _roomHash, playlistVideo);
        await Task.Delay(100);

        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.PlaylistVideos).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        room!.PlaylistVideos.Should().BeEmpty();

        await _connection.StopAsync();
    }

    [Fact]
    public async Task DeletePlaylistVideo_AdminCanDelete_ShouldRemoveAndBroadcast()
    {
        Factory.ResetSingletonState();
        Factory.ClearDatabase();

        await SetupRoomAsync();
        var db = Factory.GetDbContext();
        var room = await db.Rooms.Include(r => r.PlaylistVideos).FirstOrDefaultAsync(r => r.Hash == _roomHash);

        var video = new PlaylistVideo
        {
            Hash = "test-video-hash",
            Url = "https://www.youtube.com/watch?v=test",
            Title = "Test Video"
        };
        room!.PlaylistVideos.Add(video);
        await db.SaveChangesAsync();

        await _connection.StartAsync();

        string? deletedHash = null;
        _connection.On(HubMessages.OnDeletePlaylistVideo, (string hash) =>
        {
            deletedHash = hash;
        });

        await _connection.InvokeAsync(HubMessages.DeletePlaylistVideo, _roomHash, "test-video-hash");
        await Task.Delay(100);

        deletedHash.Should().Be("test-video-hash");
        var freshDb = Factory.GetDbContext();
        var updatedRoom = await freshDb.Rooms.Include(r => r.PlaylistVideos).FirstOrDefaultAsync(r => r.Hash == _roomHash);
        updatedRoom!.PlaylistVideos.Should().BeEmpty();

        await _connection.StopAsync();
    }
}
