using DotnetServer.Core.Entities;
using DotnetServer.Infrastructure.Repositories;
using DotnetServer.SignalR;
using Microsoft.AspNetCore.SignalR;

namespace DotnetServer.Core.Services;

public class PlaylistService : IPlaylistService
{
    private readonly AppDbContext _appData;
    private readonly ILogger<PlaylistService> _logger;
    private readonly IRoomRepository _roomRepository;
    private readonly IPlaylistRepository _playlistRepository;
    private readonly IHubContext<AppHub> _hubContext;
    private IYouTubeAPIService _youtubeAPIService;

    public bool IsServiceRunning { get; set; } = false;

    public PlaylistService(
        AppDbContext appData,
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
        _playlistRepository = new PlaylistRepository(_appData);
    }

    public async void StartPlaylistService(string roomHash)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} StartPlaylistService: Room does not exist.");
                return;
            }

            if (room.PlaylistVideos.Count == 0)
            {
                _logger.LogInformation($"{roomHash} StartPlaylistService: No PlaylistVideos found.");
                return;
            }

            _logger.LogInformation($"{roomHash} StartPlaylistService: Service started");
            await ManagePlaylistService(room);
        }
        catch (Exception ex)
        {
            IsServiceRunning = false;
            _logger.LogError(ex.ToString());
        }
    }

    public async Task ManagePlaylistService(Room room)
    {
        try
        {
            IsServiceRunning = true;

            while (room.PlaylistVideos.Count > 0 && room.Users.Count > 0)
            {
                PlaylistVideo currentVideo = room.PlaylistVideos.ToList()[0];

                room.VideoPlayer.PlaylistVideo.Url = currentVideo.Url;
                room.VideoPlayer.CurrentTime = 0;

                await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnSetVideoUrl, currentVideo.Url);

                room.VideoPlayer.IsPlaying = true;

                await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnSetIsVideoPlaying, true);

                await _roomRepository.UpdateRoomAsync(room);

                bool hasVideoEndedSuccessfully = await UpdateCurrentTime(room, currentVideo);

                room.VideoPlayer.IsPlaying = false;
                await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnSetIsVideoPlaying, false);

                if (hasVideoEndedSuccessfully)
                {
                    await _playlistRepository.DeletePlaylistVideoAsync(room.Hash, currentVideo.Hash);
                    await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnDeletePlaylistVideo, currentVideo.Hash);

                    room.VideoPlayer.PlaylistVideo.Url = null;
                    await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnSetVideoUrl, null);
                }
                else
                {
                    room.VideoPlayer.PlaylistVideo.Url = null;
                    await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnDeletePlaylistVideo, currentVideo.Hash);
                    await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnSetVideoUrl, null);
                }

                await _roomRepository.UpdateRoomAsync(room);
            }

            IsServiceRunning = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
            IsServiceRunning = false;
        }
    }

    private async Task<bool> UpdateCurrentTime(Room room, PlaylistVideo currentVideo)
    {
        try
        {
            double durationTime = await _youtubeAPIService.GetVideoDurationAsync(currentVideo.Url);

            if (durationTime == -1)
            {
                return false;
            }

            string currentVideoHash = currentVideo.Hash;

            while (room.VideoPlayer.IsPlaying && room.VideoPlayer.CurrentTime <= durationTime)
            {
                if (room.PlaylistVideos.Count == 0 ||
                    room.PlaylistVideos.ToList()[0].Hash != currentVideoHash ||
                    room.Users.Count == 0)
                {
                    return false;
                }

                await _hubContext.Clients.Group(room.Hash).SendAsync(HubMessages.OnSetPlayedSeconds, room.VideoPlayer.CurrentTime);

                await Task.Delay(TimeSpan.FromMilliseconds(500));

                room.VideoPlayer.CurrentTime += 0.5;

                await _roomRepository.UpdateRoomAsync(room);

                if (!room.VideoPlayer.IsPlaying)
                {
                    while (!room.VideoPlayer.IsPlaying)
                    {
                        if (room.PlaylistVideos.Count == 0 ||
                            room.PlaylistVideos.ToList()[0].Hash != currentVideoHash ||
                            room.Users.Count == 0)
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
            _logger.LogError(ex.ToString());
            return false;
        }
    }
}
