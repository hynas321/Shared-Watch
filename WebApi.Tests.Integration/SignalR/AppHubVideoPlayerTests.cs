using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Constants;
using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;
using WebApi.Tests.Integration.Helpers;

namespace WebApi.Tests.Integration.SignalR;

[Collection("Integration Tests")]
public class AppHubVideoPlayerTests : BaseIntegrationTest, IAsyncLifetime
{
    public AppHubVideoPlayerTests(IntegrationTestFixture fixture) : base(fixture) {}

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await CleanupConnectionsAsync();
        await Task.Delay(200);
    }

    private HubConnection _connection = null!;
    private string _roomHash = null!;
    private IVideoPlayerStateService _videoPlayerStateService = null!;

    private async Task SetupRoomAsync()
    {
        var db = Factory.GetDbContext();
        var room = new Room("TestRoom", "");
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        _roomHash = room.Hash!;

        _videoPlayerStateService = Factory.GetRequiredService<IVideoPlayerStateService>();

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
    public async Task SetIsVideoPlaying_AdminCanControlPlayback_ShouldBroadcast()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        bool? receivedState = null;
        _connection.On(HubMessages.OnSetIsVideoPlaying, (bool isPlaying) =>
        {
            receivedState = isPlaying;
        });

        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, true);
        await Task.Delay(100);

        receivedState.Should().BeTrue();
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeTrue();

        receivedState = null;
        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, false);
        await Task.Delay(100);

        receivedState.Should().BeFalse();
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeFalse();

        await _connection.StopAsync();
    }

    [Fact]
    public async Task SetPlayedSeconds_AdminCanSeekVideo_ShouldUpdateState()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        await _connection.InvokeAsync(HubMessages.SetPlayedSeconds, _roomHash, 45.5);
        await Task.Delay(100);

        _videoPlayerStateService.GetCurrentTime(_roomHash).Should().Be(45.5);

        await _connection.StopAsync();
    }

    [Fact]
    public async Task VideoPlayerState_PersistenceAcrossMultipleOperations()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, true);
        await Task.Delay(100);
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeTrue();

        await _connection.InvokeAsync(HubMessages.SetPlayedSeconds, _roomHash, 45.0);
        await Task.Delay(100);
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeTrue();
        _videoPlayerStateService.GetCurrentTime(_roomHash).Should().Be(45.0);

        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, false);
        await Task.Delay(100);
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeFalse();
        _videoPlayerStateService.GetCurrentTime(_roomHash).Should().Be(45.0);

        await _connection.StopAsync();
    }
}
