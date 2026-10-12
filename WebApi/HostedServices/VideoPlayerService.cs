using Microsoft.AspNetCore.SignalR;
using WebApi.Application.Services;
using WebApi.Application.Repositories;
using WebApi.Core.Entities;
using StackExchange.Redis;
using WebApi.SignalR;

namespace WebApi.HostedServices;

public class VideoPlayerService : IVideoPlayerService, IHostedService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<VideoPlayerService> _logger;
    private readonly IHubContext<AppHub> _hubContext;
    private readonly IYouTubeAPIService _youtubeAPIService;
    private readonly IVideoPlayerStateService _videoStateService;
    private readonly IDatabase? _redis;
    private readonly IUploadedVideoStorage? _uploadedVideoStorage;
    private readonly IRoomActivityTracker? _roomActivityTracker;
    private static readonly TimeSpan WorkerLeaseDuration = TimeSpan.FromMinutes(2);
    private readonly object _workersLock = new();
    private readonly Dictionary<string, (CancellationTokenSource Cancellation, Task Task)> _workers = new();
    private readonly CancellationTokenSource _shutdown = new();

    public bool IsServiceRunning { get; set; }

    public VideoPlayerService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<VideoPlayerService> logger,
        IHubContext<AppHub> hubContext,
        IYouTubeAPIService youTubeAPIService,
        IVideoPlayerStateService videoStateService,
        IConnectionMultiplexer? connectionMultiplexer = null,
        IUploadedVideoStorage? uploadedVideoStorage = null,
        IRoomActivityTracker? roomActivityTracker = null)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _hubContext = hubContext;
        _youtubeAPIService = youTubeAPIService;
        _videoStateService = videoStateService;
        _redis = connectionMultiplexer?.GetDatabase();
        _uploadedVideoStorage = uploadedVideoStorage;
        _roomActivityTracker = roomActivityTracker;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _shutdown.Cancel();
        Task[] workers;
        lock (_workersLock)
        {
            workers = _workers.Values.Select(worker => worker.Task).ToArray();
        }

        try
        {
            await Task.WhenAll(workers).WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || _shutdown.IsCancellationRequested)
        {
        }
    }

    public void StartPlaylistService(string roomHash)
    {
        lock (_workersLock)
        {
            if (_workers.TryGetValue(roomHash, out var existing) && !existing.Task.IsCompleted)
            {
                return;
            }

            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_shutdown.Token);
            var task = RunRoomPlaylistAsync(roomHash, cancellation);
            _workers[roomHash] = (cancellation, task);
            IsServiceRunning = true;
        }
    }

    public void StopPlaylistService(string roomHash)
    {
        CancellationTokenSource? cancellation = null;
        lock (_workersLock)
        {
            if (_workers.TryGetValue(roomHash, out var worker))
            {
                cancellation = worker.Cancellation;
            }
        }

        cancellation?.Cancel();
    }

    private async Task RunRoomPlaylistAsync(string roomHash, CancellationTokenSource cancellation)
    {
        await Task.Yield();
        var token = cancellation.Token;
        var leaseToken = Guid.NewGuid().ToString("N");
        var leaseKey = (RedisKey)$"shared-watch:playback-worker:{roomHash}";
        var ownsLease = false;
        string? durationVideoHash = null;
        var currentVideoDuration = -1;
        try
        {
            _logger.LogInformation("Starting playlist worker for room {RoomHash}.", roomHash);

            while (!token.IsCancellationRequested)
            {
                if (_redis is null)
                {
                    ownsLease = true;
                }
                else if (ownsLease)
                {
                    ownsLease = await RenewWorkerLeaseAsync(leaseKey, leaseToken);
                }

                if (!ownsLease)
                {
                    ownsLease = _redis is null || await _redis.StringSetAsync(leaseKey, leaseToken, WorkerLeaseDuration, When.NotExists);
                    if (!ownsLease)
                    {
                        await Task.Delay(SyncInterval, token);
                        continue;
                    }
                }

                var room = await GetRoomAsync(roomHash, token);
                if (_redis is not null && !await RenewWorkerLeaseAsync(leaseKey, leaseToken))
                {
                    ownsLease = false;
                    await Task.Delay(SyncInterval, token);
                    continue;
                }

                if (room is null || room.Users.Count == 0)
                {
                    _videoStateService.RemoveRoom(roomHash);
                    return;
                }

                var currentVideo = _videoStateService.GetCurrentVideo(roomHash);
                if (currentVideo is not null && !room.PlaylistVideos.Any(video => video.Hash == currentVideo.Hash))
                {
                    _videoStateService.SetCurrentVideo(roomHash, null);
                    durationVideoHash = null;
                    currentVideo = null;
                }

                var invalidVideos = room.PlaylistVideos
                    .Where(video => !IsPlayableVideo(video))
                    .ToList();

                if (invalidVideos.Count > 0)
                {
                    foreach (var invalidVideo in invalidVideos)
                    {
                        if (!string.IsNullOrWhiteSpace(invalidVideo.Hash))
                        {
                            await RemovePlaylistVideoAsync(roomHash, invalidVideo.Hash, token);
                        }
                    }

                    _videoStateService.SetCurrentVideo(roomHash, null);
                    durationVideoHash = null;
                    await Task.Delay(SyncInterval, token);
                    continue;
                }

                if (currentVideo is null && room.PlaylistVideos.Count > 0)
                {
                    currentVideo = room.PlaylistVideos.First();
                    durationVideoHash = null;
                    _videoStateService.SetCurrentVideo(roomHash, currentVideo);
                    _videoStateService.SetIsPlaying(roomHash, true);
                    await _hubContext.Clients.Group(roomHash)
                        .SendAsync(HubMessages.OnSetVideoUrl, currentVideo.Url, token);
                    await BroadcastPlaybackAsync(roomHash, token);
                }

                if (currentVideo is null)
                {
                    await BroadcastPlaybackAsync(roomHash, token);
                    await Task.Delay(SyncInterval, token);
                    continue;
                }

                if (currentVideo is not null)
                {
                    if (_videoStateService.GetIsPlaying(roomHash))
                    {
                        await (_roomActivityTracker?.TouchAsync(roomHash) ?? Task.CompletedTask);
                    }

                    if (durationVideoHash != currentVideo.Hash)
                    {
                        durationVideoHash = currentVideo.Hash;
                        try
                        {
                            currentVideoDuration = currentVideo.Duration > 0
                                ? (int)Math.Ceiling(currentVideo.Duration)
                                : await _youtubeAPIService.GetVideoDurationAsync(currentVideo.Url ?? string.Empty, token);
                        }
                        catch (OperationCanceledException) when (token.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception exception)
                        {
                            currentVideoDuration = -1;
                            _logger.LogWarning(exception, "Could not read video duration for room {RoomHash}, video {VideoHash}.", roomHash, currentVideo.Hash);
                        }
                    }

                    if (_redis is not null && !await RenewWorkerLeaseAsync(leaseKey, leaseToken))
                    {
                        ownsLease = false;
                        await Task.Delay(SyncInterval, token);
                        continue;
                    }

                    await BroadcastPlaybackAsync(roomHash, token);

                    if (currentVideoDuration > 0 && _videoStateService.GetCurrentTime(roomHash) >= currentVideoDuration)
                    {
                        if (_redis is not null && !await RenewWorkerLeaseAsync(leaseKey, leaseToken))
                        {
                            ownsLease = false;
                            await Task.Delay(SyncInterval, token);
                            continue;
                        }

                        await RemovePlaylistVideoAsync(roomHash, currentVideo.Hash ?? string.Empty, token);
                        _videoStateService.SetCurrentVideo(roomHash, null);
                        durationVideoHash = null;
                        await BroadcastPlaybackAsync(roomHash, token);
                    }
                }

                await Task.Delay(SyncInterval, token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            _logger.LogDebug("Playlist worker stopped for room {RoomHash}.", roomHash);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Playlist worker failed for room {RoomHash}.", roomHash);
        }
        finally
        {
            if (ownsLease && _redis is not null)
            {
                await _redis.ScriptEvaluateAsync(
                    "if redis.call('GET', KEYS[1]) == ARGV[1] then return redis.call('DEL', KEYS[1]) else return 0 end",
                    [leaseKey], [leaseToken]);
            }

            lock (_workersLock)
            {
                if (_workers.TryGetValue(roomHash, out var worker) && ReferenceEquals(worker.Cancellation, cancellation))
                {
                    _workers.Remove(roomHash);
                }

                IsServiceRunning = _workers.Count > 0;
            }

            cancellation.Dispose();
        }
    }

    private async Task<Room?> GetRoomAsync(string roomHash, CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
        return await roomRepository.GetRoomAsync(roomHash, cancellationToken);
    }

    private async Task<bool> RenewWorkerLeaseAsync(RedisKey leaseKey, string leaseToken)
    {
        if (_redis is null)
        {
            return true;
        }

        var renewed = await _redis.ScriptEvaluateAsync(
            "if redis.call('GET', KEYS[1]) == ARGV[1] then return redis.call('PEXPIRE', KEYS[1], ARGV[2]) else return 0 end",
            [leaseKey],
            [leaseToken, (long)WorkerLeaseDuration.TotalMilliseconds]);
        return (long)renewed == 1;
    }

    private static bool IsPlayableVideo(PlaylistVideo video) =>
        !string.IsNullOrWhiteSpace(video.Hash)
        && !string.IsNullOrWhiteSpace(video.Url)
        && !string.IsNullOrWhiteSpace(video.Title)
        && video.Duration > 0;

    private Task BroadcastPlaybackAsync(string roomHash, CancellationToken cancellationToken)
    {
        var snapshot = _videoStateService.GetVideoPlayer(roomHash);
        return _hubContext.Clients.Group(roomHash)
            .SendAsync(HubMessages.OnPlaybackSync, snapshot, cancellationToken);
    }

    public async Task<PlaylistVideo?> RemovePlaylistVideoAsync(
        string roomHash,
        string videoHash,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playlistRepository = scope.ServiceProvider.GetRequiredService<IPlaylistRepository>();
        var playlistVideo = await playlistRepository.DeletePlaylistVideoAsync(roomHash, videoHash, cancellationToken);

        if (playlistVideo is null)
        {
            return null;
        }

        if (_uploadedVideoStorage is not null && playlistVideo.Url?.StartsWith("/api/VideoFiles/", StringComparison.Ordinal) == true)
        {
            var fileName = Path.GetFileName(playlistVideo.Url);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try
                {
                    await _uploadedVideoStorage.DeleteAsync(fileName, cancellationToken);
                }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    _logger.LogWarning(exception, "Could not remove uploaded video file {FileName}.", fileName);
                }
            }
        }

        _videoStateService.SetIsCurrentVideoRemoved(roomHash, true);
        await _hubContext.Clients.Group(roomHash)
            .SendAsync(HubMessages.OnDeletePlaylistVideo, videoHash, cancellationToken);
        return playlistVideo;
    }
}

