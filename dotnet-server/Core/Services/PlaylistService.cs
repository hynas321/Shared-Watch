using Dotnet.Server.Hubs;
using dotnet_server.Infrastructure;
using Microsoft.AspNetCore.SignalR;

public class PlaylistService : IPlaylistService
{
    private readonly AppData _appData;
    private readonly ILogger<PlaylistService> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IHubContext<AppHub> _hubContext;
    private IYouTubeAPIService _youtubeAPIService;

    public bool IsServiceRunning { get; set; } = false;

    public PlaylistService(
        AppData appData,
        ILogger<PlaylistService> logger,
        IRoomRepository roomRepository,
        IPlaylistRepository playlistRepository,
        IHubContext<AppHub> hubContext,
        IYouTubeAPIService youTubeAPIService
    )
    {
        _appData = appData;
        _logger = logger;
        _roomRepository = roomRepository;
        _playlistRepository = playlistRepository;
        _hubContext = hubContext;
        _youtubeAPIService = youTubeAPIService;

        _roomRepository = new RoomRepository(_appData);
        _playlistRepository = new PlaylistRepository(_roomRepository);
    }

    public async void StartPlaylistService(string roomHash)
    {
        try
        {
            Room room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                LogRoomDoesNotExist(roomHash);
                return;
            }

            if (room.PlaylistVideos.Count == 0)
            {
                LogNoPlaylistVideos(roomHash);
                return;
            }

            await ManagePlaylistService(room);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private void LogRoomDoesNotExist(string roomHash)
    {
        _logger.LogInformation($"{roomHash} PlaylistService: Room does not exist.");
    }

    private void LogNoPlaylistVideos(string roomHash)
    {
        _logger.LogInformation($"{roomHash} PlaylistService: No PlaylistVideos found.");
    }

    private void HandleException(Exception ex)
    {
        IsServiceRunning = false;
        _logger.LogError(ex.ToString());
    }

    public async Task ManagePlaylistService(Room room)
    {
        try
        {
            IsServiceRunning = true;

            while (room.PlaylistVideos.Count != 0 && room.Users.Count != 0)
            {
                await HandleCurrentVideo(room);
            }

            IsServiceRunning = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    private async Task HandleCurrentVideo(Room room)
    {
        PlaylistVideo currentVideo = room.PlaylistVideos.ElementAt(0);
        SetVideoPlayerState(room, currentVideo.Url);

        await NotifyClientsOfVideoChange(room, currentVideo.Url, true);

        bool hasVideoEndedSuccessfully = await UpdateCurrentTime(room, currentVideo);

        await HandleVideoEnd(room, currentVideo, hasVideoEndedSuccessfully);
    }

    private void SetVideoPlayerState(Room room, string videoUrl)
    {
        room.VideoPlayerState.PlaylistVideo.Url = videoUrl;
        room.VideoPlayerState.CurrentTime = 0;
        room.VideoPlayerState.IsPlaying = true;
    }

    private async Task NotifyClientsOfVideoChange(Room room, string videoUrl, bool isPlaying)
    {
        await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetVideoUrl, videoUrl);
        await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, isPlaying);
    }

    private async Task HandleVideoEnd(Room room, PlaylistVideo currentVideo, bool hasVideoEndedSuccessfully)
    {
        room.VideoPlayerState.IsPlaying = false;
        await NotifyClientsOfVideoChange(room, null, false);

        if (hasVideoEndedSuccessfully)
        {
            _playlistRepository.DeletePlaylistVideo(room.RoomHash, currentVideo.Hash);
            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnDeletePlaylistVideo, currentVideo.Hash);
        }
        else
        {
            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnDeletePlaylistVideo, currentVideo.Hash);
        }

        room.VideoPlayerState.PlaylistVideo.Url = null;
        await NotifyClientsOfVideoChange(room, null, false);
    }

    private async Task<bool> UpdateCurrentTime(Room room, PlaylistVideo currentVideo)
    {
        try
        {
            double durationTime = _youtubeAPIService.GetVideoDuration(currentVideo.Url);

            if (durationTime == -1)
            {
                return false;
            }

            return await UpdateVideoPlayback(room, currentVideo, durationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            return false;
        }
    }

    private async Task<bool> UpdateVideoPlayback(Room room, PlaylistVideo currentVideo, double durationTime)
    {
        string currentVideoHash = currentVideo.Hash;

        while (room.VideoPlayerState.IsPlaying && room.VideoPlayerState.CurrentTime <= durationTime)
        {
            if (ShouldStopPlayback(room, currentVideoHash))
            {
                return false;
            }

            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetPlayedSeconds, room.VideoPlayerState.CurrentTime);
            await Task.Delay(TimeSpan.FromMilliseconds(500));
            room.VideoPlayerState.CurrentTime += 0.5;

            await HandlePausedPlayback(room, currentVideoHash);
        }

        return true;
    }

    private bool ShouldStopPlayback(Room room, string currentVideoHash)
    {
        return room.PlaylistVideos.Count == 0 || room.PlaylistVideos.ElementAt(0).Hash != currentVideoHash || room.Users.Count == 0;
    }

    private async Task HandlePausedPlayback(Room room, string currentVideoHash)
    {
        while (!room.VideoPlayerState.IsPlaying)
        {
            if (ShouldStopPlayback(room, currentVideoHash))
            {
                return;
            }

            await Task.Delay(100);
        }
    }
}
