using Dotnet.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

public class PlaylistService : IPlaylistService
{
    private readonly ILogger<PlaylistService> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly IPlaylistRepository _playlistManager;
    private readonly IHubContext<AppHub> _hubContext;
    private IYouTubeAPIService _youtubeAPIService;
    
    public bool IsHandlerRunning { get; set; } = false;

    public PlaylistService(
        ILogger<PlaylistService> logger,
        IHubContext<AppHub> hubContext,
        IYouTubeAPIService youTubeAPIService

    )
    {
        _logger = logger;
        _hubContext = hubContext;
        _youtubeAPIService = youTubeAPIService;

        _roomRepository = new RoomRepository();
        _playlistManager = new PlaylistRepository(_roomRepository);
    }

    public async void StartPlaylistService(string roomHash)
    {
        try
        {
            IRoom room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} StartPlaylistHandler: Room does not exist.");
                return;
            }

            if (!room.PlaylistVideos.Any())
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

    public async Task ManagePlaylistService(IRoom room)
    {
        try
        {
            IsHandlerRunning = true;

            while (room.PlaylistVideos.Any() && room.Users.Any())
            {
                IPlaylistVideo currentVideo = room.PlaylistVideos.ElementAt(0);

                room.VideoPlayerState.PlaylistVideo.Url = currentVideo.Url;
                room.VideoPlayerState.CurrentTime = 0;

                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetVideoUrl, currentVideo.Url);

                room.VideoPlayerState.IsPlaying = true;

                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, true);

                bool hasVideoEndedSuccessfully = await UpdateCurrentTime(room, currentVideo);

                room.VideoPlayerState.IsPlaying = false;
                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, false);

                if (hasVideoEndedSuccessfully)
                {
                    _playlistManager.DeletePlaylistVideo(room.RoomHash, currentVideo.Hash);
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnDeletePlaylistVideo, currentVideo.Hash);

                    room.VideoPlayerState.PlaylistVideo.Url = null;
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetVideoUrl, null);
                }
                else
                {
                    room.VideoPlayerState.PlaylistVideo.Url = null;
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnDeletePlaylistVideo, currentVideo.Hash);
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetVideoUrl, null);
                }
            }

            IsHandlerRunning = false;
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.ToString());
        }
    }

    private async Task<bool> UpdateCurrentTime(IRoom room, IPlaylistVideo currentVideo)
    {
        try
        {
            double durationTime = _youtubeAPIService.GetVideoDuration(currentVideo.Url);

            if (durationTime == -1)
            {
                return false;
            }

            string currentVideoHash = currentVideo.Hash;

            while (room.VideoPlayerState.IsPlaying && room.VideoPlayerState.CurrentTime <= durationTime)
            {
                if (!room.PlaylistVideos.Any() ||
                    room.PlaylistVideos.ElementAt(0).Hash != currentVideoHash ||
                    !room.Users.Any()
                )
                {
                    return false;
                }

                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubMessages.OnSetPlayedSeconds, room.VideoPlayerState.CurrentTime);

                await Task.Delay(TimeSpan.FromMilliseconds(500));

                room.VideoPlayerState.CurrentTime += 0.5;

                if (!room.VideoPlayerState.IsPlaying)
                {
                    while (!room.VideoPlayerState.IsPlaying)
                    {
                        if (!room.PlaylistVideos.Any() ||
                            room.PlaylistVideos.ElementAt(0).Hash != currentVideoHash ||
                            !room.Users.Any()
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