using Dotnet.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

public class PlaylistHandler
{
    private readonly ILogger<PlaylistHandler> _logger;
    private readonly RoomManager _roomManager = new RoomManager();
    private readonly IHubContext<RoomHub> _hubContext;
    
    public bool IsHandlerRunning {get; set; } = false;

    public PlaylistHandler(ILogger<PlaylistHandler> logger, IHubContext<RoomHub> hubContext)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public async void StartPlaylistHandler(string roomHash)
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

            await ManagePlaylistHandler(room);
        }
        catch (Exception)
        {
            IsHandlerRunning = false;
        }
    }

    public async Task ManagePlaylistHandler(Room room)
    {
        IsHandlerRunning = true;

        while (room.PlaylistVideos.Count > 0)
        {
            PlaylistVideo currentVideo = room.PlaylistVideos[0];

            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetVideoUrl, currentVideo.Url);

            room.VideoPlayerState.PlaylistVideo.Url = currentVideo.Url;
            room.VideoPlayerState.CurrentTime = 0;
            room.VideoPlayerState.Duration = 20; //To be changed
            room.VideoPlayerState.IsPlaying = true;

            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetIsVideoPlaying, true);

            bool hasVideoEndedSuccessfully = await UpdatePlayedSeconds(room);

            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetIsVideoPlaying, false);

            if (hasVideoEndedSuccessfully)
            {
                _roomManager.DeletePlaylistVideo(room.RoomHash, 0);
                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnDeletePlaylistVideo, 0);
                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetVideoUrl, null);
            }

            if (!hasVideoEndedSuccessfully)
            {
                try
                {
                    _roomManager.DeletePlaylistVideo(room.RoomHash, 0);
                }
                catch
                {
                    // Handle the exception, log, or take appropriate action
                }
                finally
                {
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnDeletePlaylistVideo, 0);
                    await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetVideoUrl, null);
                }
            }
        }

        IsHandlerRunning = false;
    }

    private async Task<bool> UpdatePlayedSeconds(Room room)
    {
        try
        {
            double durationTime = room.VideoPlayerState.Duration;
            string currentVideoUrl = room.VideoPlayerState.PlaylistVideo.Url;

            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            while (room.VideoPlayerState.IsPlaying && room.VideoPlayerState.CurrentTime <= durationTime)
            {
                _logger.LogInformation(room.VideoPlayerState.CurrentTime.ToString());

                if (room.VideoPlayerState.PlaylistVideo.Url != currentVideoUrl)
                {
                    cancellationToken.Cancel();
                    return false;
                }

                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetPlayedSeconds, room.VideoPlayerState.CurrentTime);

                room.VideoPlayerState.CurrentTime++;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);
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