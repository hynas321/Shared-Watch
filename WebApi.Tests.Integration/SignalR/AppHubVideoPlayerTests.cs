using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using WebApi.Application.Constants;
using WebApi.Application.Services;
using WebApi.Core.Entities;
using WebApi.Core.Entities.In_memory;
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
    private HubConnection? _secondConnection;
    private string _roomHash = null!;
    private IVideoPlayerStateService _videoPlayerStateService = null!;

    private async Task SetupRoomAsync()
    {
        var db = Factory.GetDbContext();
        var room = new Room("TestRoom", "");
        room.Users.Add(new User("AdminUser", Role.Admin));
        room.Users.Add(new User("SecondUser", Role.User));
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

        if (_secondConnection != null)
        {
            try
            {
                if (_secondConnection.State == HubConnectionState.Connected)
                {
                    await _secondConnection.StopAsync().ConfigureAwait(false);
                }
            }
            catch { }

            try
            {
                await _secondConnection.DisposeAsync().ConfigureAwait(false);
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

        var jwtService = Factory.GetRequiredService<IJwtTokenService>();
        var secondUserToken = jwtService.GenerateToken("SecondUser", Role.User, _roomHash);
        _secondConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/Hub/Room", options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Headers.Add("Authorization", $"Bearer {secondUserToken}");
            })
            .Build();
        await _secondConnection.StartAsync();

        var receivedState = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondUserReceivedState = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _connection.On(HubMessages.OnPlaybackSync, (VideoPlayer? snapshot) =>
        {
            if (snapshot != null)
            {
                receivedState.TrySetResult(snapshot);
            }
        });
        _secondConnection.On(HubMessages.OnPlaybackSync, (VideoPlayer? snapshot) =>
        {
            if (snapshot != null)
            {
                secondUserReceivedState.TrySetResult(snapshot);
            }
        });

        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, true);

        var snapshots = await Task.WhenAll(
            receivedState.Task.WaitAsync(TimeSpan.FromSeconds(3)),
            secondUserReceivedState.Task.WaitAsync(TimeSpan.FromSeconds(3)));
        snapshots.Should().OnlyContain(snapshot => snapshot != null && snapshot.IsPlaying);
        snapshots.Select(snapshot => snapshot!.Revision).Distinct().Should().ContainSingle();
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeTrue();

        receivedState = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        secondUserReceivedState = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, false);

        snapshots = await Task.WhenAll(
            receivedState.Task.WaitAsync(TimeSpan.FromSeconds(3)),
            secondUserReceivedState.Task.WaitAsync(TimeSpan.FromSeconds(3)));
        snapshots.Should().OnlyContain(snapshot => snapshot != null && !snapshot.IsPlaying);
        snapshots.Select(snapshot => snapshot!.Revision).Distinct().Should().ContainSingle();
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

        _videoPlayerStateService.GetCurrentTime(_roomHash).Should().BeApproximately(45.5, 0.1);

        await _connection.StopAsync();
    }

    [Fact]
    public async Task SetPlayedSeconds_WhilePlaying_ShouldKeepPlaybackRunningForAllClients()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();
        await _connection.StartAsync();

        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, true);
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeTrue();

        var seekedSnapshot = new TaskCompletionSource<VideoPlayer?>(
            TaskCreationOptions.RunContinuationsAsynchronously);
        _connection.On(HubMessages.OnPlaybackSync, (VideoPlayer? snapshot) =>
        {
            if (snapshot is not null && snapshot.CurrentTime >= 45)
            {
                seekedSnapshot.TrySetResult(snapshot);
            }
        });

        await _connection.InvokeAsync(HubMessages.SetPlayedSeconds, _roomHash, 45.5);
        var received = await seekedSnapshot.Task.WaitAsync(TimeSpan.FromSeconds(3));

        received.Should().NotBeNull();
        received!.IsPlaying.Should().BeTrue();
        received.CurrentTime.Should().BeApproximately(45.5, 0.1);
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeTrue();

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
        _videoPlayerStateService.GetCurrentTime(_roomHash).Should().BeApproximately(45.0, 0.5);

        await _connection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, false);
        await Task.Delay(100);
        _videoPlayerStateService.GetIsPlaying(_roomHash).Should().BeFalse();
        _videoPlayerStateService.GetCurrentTime(_roomHash).Should().BeApproximately(45.0, 0.5);

        await _connection.StopAsync();
    }

    [Fact]
    public async Task PlaylistWorker_WhenVideoEnds_RemovesItAndStartsNextVideo()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();

        var db = Factory.GetDbContext();
        var playlist = db.Rooms.Single(room => room.Hash == _roomHash).PlaylistVideos;
        playlist.Add(
            new PlaylistVideo
            {
                Hash = "first-video",
                Url = $"/api/VideoFiles/{_roomHash}/first.mp4",
                Duration = 1
            });
        playlist.Add(
            new PlaylistVideo
            {
                Hash = "second-video",
                Url = $"/api/VideoFiles/{_roomHash}/second.mp4",
                Duration = 30
            });
        await db.SaveChangesAsync();

        var firstVideoStarted = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var videoRemoved = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondVideoStarted = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _connection.On(HubMessages.OnPlaybackSync, (VideoPlayer? snapshot) =>
        {
            if (snapshot?.PlaylistVideo?.Hash == "first-video")
            {
                firstVideoStarted.TrySetResult(snapshot);
            }

            if (snapshot?.PlaylistVideo?.Hash == "second-video")
            {
                secondVideoStarted.TrySetResult(snapshot);
            }
        });
        _connection.On(HubMessages.OnDeletePlaylistVideo, (string hash) => videoRemoved.TrySetResult(hash));

        await _connection.StartAsync();
        await firstVideoStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await _connection.InvokeAsync(HubMessages.SetPlayedSeconds, _roomHash, 1.0);

        (await videoRemoved.Task.WaitAsync(TimeSpan.FromSeconds(5))).Should().Be("first-video");
        var nextSnapshot = await secondVideoStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        nextSnapshot.Should().NotBeNull();
        nextSnapshot!.IsPlaying.Should().BeTrue();

        var updatedRoom = await Factory.GetDbContext().Rooms
            .Include(room => room.PlaylistVideos)
            .SingleAsync(room => room.Hash == _roomHash);
        updatedRoom.PlaylistVideos.Select(video => video.Hash).Should().BeEquivalentTo(["second-video"]);
    }

    [Fact]
    public async Task PlaylistWorker_WhenLastVideoEnds_RemovesItAndClearsPlayer()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();

        var db = Factory.GetDbContext();
        db.Rooms.Single(room => room.Hash == _roomHash).PlaylistVideos.Add(
            new PlaylistVideo
            {
                Hash = "only-video",
                Url = $"/api/VideoFiles/{_roomHash}/only.mp4",
                Duration = 1
            });
        await db.SaveChangesAsync();

        var firstVideoStarted = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var clearedSnapshot = new TaskCompletionSource<VideoPlayer?>(TaskCreationOptions.RunContinuationsAsynchronously);
        _connection.On(HubMessages.OnPlaybackSync, (VideoPlayer? snapshot) =>
        {
            if (snapshot?.PlaylistVideo?.Hash == "only-video")
            {
                firstVideoStarted.TrySetResult(snapshot);
            }

            if (snapshot?.PlaylistVideo is null && snapshot is not null)
            {
                clearedSnapshot.TrySetResult(snapshot);
            }
        });

        await _connection.StartAsync();
        await firstVideoStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await _connection.InvokeAsync(HubMessages.SetPlayedSeconds, _roomHash, 1.0);

        var snapshotAfterEnd = await clearedSnapshot.Task.WaitAsync(TimeSpan.FromSeconds(5));
        snapshotAfterEnd!.IsPlaying.Should().BeFalse();
        (await Factory.GetDbContext().Rooms
            .Include(room => room.PlaylistVideos)
            .SingleAsync(room => room.Hash == _roomHash))
            .PlaylistVideos.Should().BeEmpty();
    }

    [Fact]
    public async Task MultipleConnections_WhenOneDisconnects_ShouldKeepRoomAccessForTheOther()
    {
        Factory.ResetSingletonState();
        await SetupRoomAsync();

        var jwtService = Factory.GetRequiredService<IJwtTokenService>();
        var token = jwtService.GenerateToken("AdminUser", Role.Admin, _roomHash);
        _secondConnection = new HubConnectionBuilder()
            .WithUrl("http://localhost/Hub/Room", options =>
            {
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.Headers.Add("Authorization", $"Bearer {token}");
            })
            .Build();

        await _connection.StartAsync();
        var secondConnection = _secondConnection!;
        await secondConnection.StartAsync();
        await _connection.StopAsync();
        await Task.Delay(TimeSpan.FromSeconds(7));

        await secondConnection.InvokeAsync(HubMessages.SetIsVideoPlaying, _roomHash, true);
        var snapshot = await secondConnection.InvokeAsync<VideoPlayer?>(
            HubMessages.GetPlaybackSnapshot,
            _roomHash);
        snapshot.Should().NotBeNull();

        var room = await Factory.GetDbContext().Rooms
            .Include(currentRoom => currentRoom.Users)
            .SingleAsync(currentRoom => currentRoom.Hash == _roomHash);
        room.Users.Should().ContainSingle(user => user.Username == "AdminUser");
    }
}
