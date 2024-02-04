using Dotnet.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

public class PlaylistService
{
    private readonly ILogger<PlaylistService> _logger;
    private readonly RoomManager _roomManager = new RoomManager();
    private readonly PlaylistManager _playlistManager = new PlaylistManager();
    private readonly IHubContext<AppHub> _hubContext;
    private YouTubeAPIService _youtubeAPIService;
    
    public bool IsHandlerRunning {get; set; } = false;

    public PlaylistService(
        ILogger<PlaylistService> logger,
        IHubContext<AppHub> hubContext,
        YouTubeAPIService youTubeAPIService

    )
    {
        _logger = logger;
        _hubContext = hubContext;
        _youtubeAPIService = youTubeAPIService;
    }

    public async void StartPlaylistService(string roomHash)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} StartPlaylistHandler: Room does not exist.");
                return;
            }

            if (room.PlaylistVideos.Count == 0)
            {
                _logger.LogInformation($"{roomHash} StartPlaylistHandler: No PlaylistVideos found.");
                return;
            }

            await ManagePlaylistService(room);
        }
        catch (Exception ex)
        {
            IsHandlerRunning = false;
            _logger.LogInformation(ex.ToString());
        }
    }

    public async Task ManagePlaylistService(Room room)
    {
        try
        {
            IsHandlerRunning = true;

            while (room.PlaylistVideos.Count > 0 && room.Users.Count > 0)
            {
                PlaylistVideo currentVideo = room.PlaylistVideos[0];

                room.VideoPlayerState.PlaylistVideo.Url = currentVideo.Url;
                room.VideoPlayerState.CurrentTime = 0;

                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetVideoUrl, currentVideo.Url);

                room.VideoPlayerState.IsPlaying = true;

                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetIsVideoPlaying, true);

                bool hasVideoEndedSuccessfully = await UpdateCurrentTime(room, currentVideo);

                room.VideoPlayerState.IsPlaying = false;
                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetIsVideoPlaying, false);

                if (hasVideoEndedSuccessfully)
                {
                    _playlistManager.DeletePlaylistVideo(room.RoomHash, currentVideo.Hash);
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnDeletePlaylistVideo, currentVideo.Hash);

                    room.VideoPlayerState.PlaylistVideo.Url = null;
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetVideoUrl, null);
                }
                else
                {
                    room.VideoPlayerState.PlaylistVideo.Url = null;
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnDeletePlaylistVideo, currentVideo.Hash);
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetVideoUrl, null);
                }
            }

            IsHandlerRunning = false;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.ToString());
        }
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

            string currentVideoHash = currentVideo.Hash;

            _logger.LogInformation(durationTime.ToString() + " duration");

            while (room.VideoPlayerState.IsPlaying && room.VideoPlayerState.CurrentTime <= durationTime)
            {
                _logger.LogInformation(room.VideoPlayerState.CurrentTime.ToString());

                if (room.PlaylistVideos.Count == 0 ||
                    room.PlaylistVideos[0].Hash != currentVideoHash ||
                    room.Users.Count == 0
                )
                {
                    return false;
                }
                
                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetPlayedSeconds, room.VideoPlayerState.CurrentTime);

                await Task.Delay(TimeSpan.FromMilliseconds(500));

                room.VideoPlayerState.CurrentTime += 0.5;

                if (!room.VideoPlayerState.IsPlaying)
                {
                    while (!room.VideoPlayerState.IsPlaying)
                    {
                        if (room.PlaylistVideos.Count == 0 ||
                            room.PlaylistVideos[0].Hash != currentVideoHash ||
                            room.Users.Count == 0
                        )
                        {
                            return false;
                        }

                        await Task.Delay(100);
                    }
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
            return false;
        }
    }
}