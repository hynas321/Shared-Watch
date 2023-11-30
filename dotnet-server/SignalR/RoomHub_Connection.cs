using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly RoomManager _roomManager = new RoomManager();
    private readonly PlaylistManager _playlistHandler;
    private readonly YouTubeAPIService _youtubeAPIService;

    public RoomHub(ILogger<RoomHub> logger, PlaylistManager playlistHandler, YouTubeAPIService youTubeAPIService)
    {
        _logger = logger;
        _playlistHandler = playlistHandler;
        _youtubeAPIService = youTubeAPIService;
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnReceiveConnectionId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        await base.OnDisconnectedAsync(exception);
    }
}