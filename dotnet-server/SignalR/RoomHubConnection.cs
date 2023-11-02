using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHubConnection : Hub
{
    private readonly ILogger<RoomHubConnection> _logger;
    private readonly RoomManager _roomManager = new RoomManager();

    public RoomHubConnection(ILogger<RoomHubConnection> _logger)
    {
        this._logger = _logger;
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