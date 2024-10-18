using Microsoft.AspNetCore.SignalR;
using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;
using WebApi.Infrastructure.Repositories;
using WebApi.SignalR;
using System.Collections.Concurrent;

public class VideoPlayerService : IVideoPlayerService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<VideoPlayerService> _logger;
    private readonly IHubContext<AppHub> _hubContext;
    private readonly IYouTubeAPIService _youtubeAPIService;
    private readonly IVideoPlayerStateService _videoStateService;

    public bool IsServiceRunning { get; set; } = false;

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokenSources = new ConcurrentDictionary<string, CancellationTokenSource>();

    public VideoPlayerService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<VideoPlayerService> logger,
        IHubContext<AppHub> hubContext,
        IYouTubeAPIService youTubeAPIService,
        IVideoPlayerStateService videoStateService
    )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _hubContext = hubContext;
        _youtubeAPIService = youTubeAPIService;
        _videoStateService = videoStateService;
    }

    public async void StartPlaylistService(string roomHash)
    {
        if (!_cancellationTokenSources.TryAdd(roomHash, new CancellationTokenSource()))
        {
            _logger.LogWarning($"{roomHash} StartPlaylistService: Service is already running.");
            return;
        }

        IsServiceRunning = true;
        CancellationTokenSource cancellationTokenSource = _cancellationTokenSources[roomHash];
        CancellationToken token = cancellationTokenSource.Token;

        try
        {
            _logger.LogInformation($"{roomHash} StartPlaylistService: Starting service.");

            using var scope = _serviceScopeFactory.CreateScope();
            var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();

            Room room = await roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                IsServiceRunning = false;
                _logger.LogWarning($"{roomHash} StartPlaylistService: Room does not exist.");
                StopPlaylistService(roomHash);
                return;
            }

            if (room.PlaylistVideos.Count == 0)
            {
                IsServiceRunning = false;
                _logger.LogWarning($"{roomHash} StartPlaylistService: No PlaylistVideos found.");
                StopPlaylistService(roomHash);
                return;
            }

            _logger.LogInformation($"{roomHash} StartPlaylistService: Service started with {room.PlaylistVideos.Count} videos.");
            await ManagePlaylistService(roomHash, token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"{roomHash} StartPlaylistService: Operation was canceled.");
        }
        catch (Exception ex)
        {
            IsServiceRunning = false;
            _logger.LogError($"{roomHash} StartPlaylistService: Exception occurred: {ex}");
        }
        finally
        {
            StopPlaylistService(roomHash);
        }
    }

    public async Task ManagePlaylistService(string roomHash, CancellationToken token)
    {
        try
        {
            _logger.LogInformation($"{roomHash} ManagePlaylistService: Starting playlist management.");
            IsServiceRunning = true;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                PlaylistVideo currentVideo = _videoStateService.GetCurrentVideo(roomHash);

                if (currentVideo == null)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var roomRepository = scope.ServiceProvider.GetRequiredService<IRoomRepository>();
                        var room = await roomRepository.GetRoomAsync(roomHash);

                        if (room == null || room.Users.Count == 0)
                        {
                            IsServiceRunning = false;
                            _logger.LogWarning($"{roomHash} ManagePlaylistService: No users connected or room does not exist. Exiting.");
                            StopPlaylistService(roomHash);
                            break;
                        }

                        if (room.PlaylistVideos.Count == 0)
                        {
                            IsServiceRunning = false;
                            StopPlaylistService(roomHash);
                            break;
                        }

                        currentVideo = room.PlaylistVideos.First();

                        _videoStateService.SetCurrentVideo(roomHash, currentVideo);
                        _videoStateService.SetIsPlaying(roomHash, true);
                        _videoStateService.SetCurrentTime(roomHash, 0);
                        _videoStateService.SetIsCurrentVideoRemoved(roomHash, false);
                    }

                    _logger.LogInformation($"{roomHash} ManagePlaylistService: Starting playback of video '{currentVideo.Hash}'.");

                    _logger.LogDebug($"{roomHash} ManagePlaylistService: Current video set to '{currentVideo.Hash}', isPlaying: true, currentTime: 0.");

                    await _hubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnSetVideoUrl, currentVideo.Url);
                    _logger.LogDebug($"{roomHash} ManagePlaylistService: Sent video URL '{currentVideo.Url}' to clients.");

                    await _hubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, true);
                    _logger.LogDebug($"{roomHash} ManagePlaylistService: Notified clients that video is playing.");

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            bool hasVideoEndedSuccessfully = await UpdateCurrentTime(roomHash, currentVideo, token);

                            _videoStateService.SetIsPlaying(roomHash, false);
                            _logger.LogDebug($"{roomHash} ManagePlaylistService: Video playback ended, isPlaying set to false.");

                            await _hubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, false);
                            _logger.LogDebug($"{roomHash} ManagePlaylistService: Notified clients that video is no longer playing.");

                            if (hasVideoEndedSuccessfully)
                            {
                                await RemovePlaylistVideoAsync(roomHash, currentVideo.Hash);
                            }

                            _videoStateService.SetCurrentVideo(roomHash, null);
                            _logger.LogDebug($"{roomHash} ManagePlaylistService: Current video set to null.");

                            await _hubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnSetVideoUrl, null);
                            _logger.LogDebug($"{roomHash} ManagePlaylistService: Sent null video URL to clients.");
                        }
                        catch (OperationCanceledException)
                        {
                            _logger.LogInformation($"{roomHash} UpdateCurrentTime: Operation was canceled.");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"{roomHash} UpdateCurrentTime: Exception occurred: {ex}");
                        }
                    }, token);
                }

                await Task.Delay(1000, token);
            }

            IsServiceRunning = false;
            _logger.LogInformation($"{roomHash} ManagePlaylistService: Playlist management ended.");
        }
        catch (OperationCanceledException)
        {
            IsServiceRunning = false;
            _logger.LogInformation($"{roomHash} ManagePlaylistService: Operation was canceled.");
        }
        catch (Exception ex)
        {
            IsServiceRunning = false;
            _logger.LogError($"{roomHash} ManagePlaylistService: Exception occurred: {ex}");
        }
    }

    private async Task<bool> UpdateCurrentTime(string roomHash, PlaylistVideo currentVideo, CancellationToken token)
    {
        double durationTime;

        try
        {
            token.ThrowIfCancellationRequested();

            _logger.LogInformation($"{roomHash} UpdateCurrentTime: Fetching video duration for URL '{currentVideo.Url}'.");

            durationTime = await _youtubeAPIService.GetVideoDurationAsync(currentVideo.Url);

            _logger.LogInformation($"{roomHash} UpdateCurrentTime: Video duration is {durationTime} seconds.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation($"{roomHash} UpdateCurrentTime: Operation was canceled during video duration fetch.");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"{roomHash} UpdateCurrentTime: Error fetching video duration: {ex}");
            return false;
        }

        if (durationTime == -1)
        {
            _logger.LogWarning($"{roomHash} UpdateCurrentTime: Invalid video duration received.");
            return false;
        }

        _logger.LogInformation($"{roomHash} UpdateCurrentTime: Starting time update loop for video '{currentVideo.Hash}'.");

        while (true)
        {
            token.ThrowIfCancellationRequested();

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var playlistRepository = scope.ServiceProvider.GetRequiredService<IPlaylistRepository>();
                var playlistVideo = await playlistRepository.GetPlaylistVideoAsync(roomHash, currentVideo.Hash);

                if (playlistVideo == null)
                {
                    _logger.LogWarning($"{roomHash} UpdateCurrentTime: Current video '{currentVideo.Hash}' no longer exists in the playlist. Exiting time update loop.");
                    return false;
                }
            }

            if (!_videoStateService.GetIsPlaying(roomHash))
            {
                _logger.LogInformation($"{roomHash} UpdateCurrentTime: Video is paused. Handling pause state.");

                bool shouldExit = await HandlePauseState(roomHash, token);

                if (shouldExit)
                {
                    _logger.LogWarning($"{roomHash} UpdateCurrentTime: Exiting time update loop due to video removal.");
                    return false;
                }
            }

            double currentTime = _videoStateService.GetCurrentTime(roomHash);

            if (currentTime >= durationTime)
            {
                _logger.LogInformation($"{roomHash} UpdateCurrentTime: Video '{currentVideo.Hash}' playback completed.");
                return true;
            }

            await _hubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnSetPlayedSeconds, currentTime);
            await Task.Delay(1000, token);

            _videoStateService.SetCurrentTime(roomHash, currentTime + 1);
        }
    }

    private async Task<bool> HandlePauseState(string roomHash, CancellationToken token)
    {
        _logger.LogInformation($"{roomHash} HandlePauseState: Handling pause state.");

        while (!_videoStateService.GetIsPlaying(roomHash))
        {
            token.ThrowIfCancellationRequested();

            if (_videoStateService.GetIsCurrentVideoRemoved(roomHash))
            {
                _logger.LogWarning($"{roomHash} HandlePauseState: Exiting because current video was removed.");
                return true;
            }

            await Task.Delay(1000, token);
        }

        _logger.LogInformation($"{roomHash} HandlePauseState: Resuming playback after pause.");
        return false;
    }

    public async Task<PlaylistVideo> RemovePlaylistVideoAsync(string roomHash, string videoHash)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var playlistRepository = scope.ServiceProvider.GetRequiredService<IPlaylistRepository>();

        var playlistVideo = await playlistRepository.DeletePlaylistVideoAsync(roomHash, videoHash);

        if (playlistVideo == null)
        {
            _logger.LogWarning($"{roomHash} RemovePlaylistVideoAsync: Video '{videoHash}' not found in playlist.");
            return null;
        }

        _videoStateService.SetIsCurrentVideoRemoved(roomHash, true);
        _logger.LogInformation($"{roomHash} RemovePlaylistVideoAsync: Video '{videoHash}' was the current video and has been removed.");

        await _hubContext.Clients.Group(roomHash).SendAsync(HubMessages.OnDeletePlaylistVideo, videoHash);

        return playlistVideo;
    }

    public void StopPlaylistService(string roomHash)
    {
        if (_cancellationTokenSources.TryRemove(roomHash, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
            IsServiceRunning = false;
            _logger.LogInformation($"{roomHash} StopPlaylistService: Playlist service has been stopped.");
        }
    }
}
