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

        while (true)
        {
            room.VideoPlayerState.PlaylistVideo.Url = room.PlaylistVideos[0].Url;
            room.VideoPlayerState.CurrentTime = 0;
            room.VideoPlayerState.Duration = 10;    //To be changed
            room.VideoPlayerState.IsPlaying = true;

            bool hasVideoEndedSuccessfully = await Task.Run(() => UpdatePlayedSeconds(room));

            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetIsVideoPlaying, false);

            if (hasVideoEndedSuccessfully)
            {
                room.PlaylistVideos.RemoveAt(0);
            }

            if (room.PlaylistVideos.Count == 0)
            {
                IsHandlerRunning = false;
                break;
            }
        }
    }

    private async Task<bool> UpdatePlayedSeconds(Room room)
    {
        try
        {
            if (room == null)
            {
                return false;
            }

            room.VideoPlayerState.CurrentTime = 0;

            double durationTime = room.VideoPlayerState.Duration;
            string currentVideoUrl = room.VideoPlayerState.PlaylistVideo.Url;

            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetIsVideoPlaying, true);

            for (int i = 0; i <= durationTime; i++)
            {
                _logger.LogInformation(i.ToString());
                if (room == null)
                {
                    cancellationToken.Cancel();
                    return false;
                }

                if (room.VideoPlayerState.PlaylistVideo.Url != currentVideoUrl)
                {
                    cancellationToken.Cancel();
                    return false;
                }

                if (room.PlaylistVideos[0] == null)
                {
                    cancellationToken.Cancel();
                    return false;
                }

                if (room.VideoPlayerState.CurrentTime >= room.VideoPlayerState.Duration)
                {
                    cancellationToken.Cancel();
                    return true;
                }

                await _hubContext.Clients.Group(room.RoomHash).SendAsync(HubEvents.OnSetPlayedSeconds, room.VideoPlayerState.CurrentTime);

                room.VideoPlayerState.CurrentTime++;

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken.Token);
            }

            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(Convert.ToString(ex));
            return false;
        }
    }
}