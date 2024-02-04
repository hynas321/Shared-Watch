using Dotnet.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

public class SessionManager
{
    private readonly ILogger<SessionManager> _logger;
    private readonly IHubContext<AppHub> _hubContext;
    private bool _isHandlerRunning = false;

    public SessionManager(
        ILogger<SessionManager> logger,
        IHubContext<AppHub> hubContext
    )
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    public void StartSessionManager(string roomHash)
    {
        if (!_isHandlerRunning)
        {
            _isHandlerRunning = true;
            Task.Run(async () => await HeartbeatLoop(roomHash));
        }
    }

    private async Task HeartbeatLoop(string roomHash)
    {
        while (_isHandlerRunning)
        {
            await Task.Delay(TimeSpan.FromMinutes(1));

            try
            {
                await _hubContext.Clients.Group(roomHash).SendAsync(HubEvents.OnSendHeartbeat);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending heartbeat: {ex.Message}");
            }
        }
    }

    public void StopSessionManager()
    {
        _isHandlerRunning = false;
    }
}