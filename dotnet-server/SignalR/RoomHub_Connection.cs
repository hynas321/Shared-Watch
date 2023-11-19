using dotnet_server.Controllers;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly RoomManager _roomManager = new RoomManager();

    public RoomHub(ILogger<RoomHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        await base.OnDisconnectedAsync(exception);
    }
}