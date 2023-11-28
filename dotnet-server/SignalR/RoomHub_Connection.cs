using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly RoomManager _roomManager = new RoomManager();
    private readonly PlaylistHandler _playlistHandler;

    public RoomHub(ILogger<RoomHub> logger, PlaylistHandler playlistHandler)
    {
        _logger = logger;
        _playlistHandler = playlistHandler;
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