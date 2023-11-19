using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.AddQueuedVideo)]
    public async Task AddQueuedVideo(string roomHash, QueuedVideo queuedVideo)
    {
        _logger.LogInformation($"{roomHash} AddQueuedVideo: {queuedVideo.Url}");

        await Clients.All.SendAsync(HubEvents.OnAddQueuedVideo, JsonHelper.Serialize(queuedVideo));
    }

    [HubMethodName(HubEvents.DeleteQueuedVideo)]
    public async Task DeleteQueuedVideo(string roomHash, QueuedVideo queuedVideo)
    {
        _logger.LogInformation($"{roomHash} DeleteQueuedVideo: {queuedVideo.Url}");

        await Clients.All.SendAsync(HubEvents.OnDeleteQueuedVideo, JsonHelper.Serialize(queuedVideo));
    }
}