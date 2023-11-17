using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    private readonly ILogger<RoomHub> _logger;
    private readonly RoomManager _roomManager = new RoomManager();

    public RoomHub(ILogger<RoomHub> _logger)
    {
        this._logger = _logger;
    }

    public override async Task OnConnectedAsync()
    {   
        await Clients.All.SendAsync(HubEvents.OnLeaveRoom, new UserDTO("Sudden user leave", false));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {   
        await Clients.All.SendAsync(HubEvents.OnLeaveRoom, new UserDTO("Sudden user leave", false));
        await base.OnDisconnectedAsync(exception);
    }
}