using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.AddQueuedVideo)]
    public async Task AddQueuedVideo(string roomHash, string authorizationToken, QueuedVideo queuedVideo)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} AddQueuedVideo: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} AddQueuedVideo: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false && room.RoomSettings.IsAddingVideosAllowed == false)
        {
            _logger.LogInformation($"{roomHash} AddQueuedVideo: User does not have the permission. Authorization Token: {authorizationToken}");
        }

        _logger.LogInformation($"{roomHash} AddQueuedVideo: {queuedVideo.Url}. Authorization Token: {authorizationToken}");

        bool isQueuedVideoAdded = _roomManager.AddQueuedVideo(roomHash, queuedVideo);

        if (!isQueuedVideoAdded)
        {
            _logger.LogInformation($"{roomHash} AddQueuedVideo: Error when adding a queued video. Authorization Token: {authorizationToken}");
        }

        await Clients.Group(roomHash).SendAsync(HubEvents.OnAddQueuedVideo, JsonHelper.Serialize(queuedVideo));
    }

    [HubMethodName(HubEvents.DeleteQueuedVideo)]
    public async Task DeleteQueuedVideo(string roomHash, string authorizationToken, int queuedVideoIndex)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} DeleteQueuedVideo: Room does not exist. Authorization Token: {authorizationToken}, QueuedVideoIndex: {queuedVideoIndex}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} DeleteQueuedVideo: User does not exist. Authorization Token: {authorizationToken}, QueuedVideoIndex: {queuedVideoIndex}");
            return;
        }

        if (user.IsAdmin == false && room.RoomSettings.IsRemovingVideosAllowed == false)
        {
            _logger.LogInformation($"{roomHash} DeleteQueuedVideo: User does not have the permission. Authorization Token: {authorizationToken}, QueuedVideoIndex: {queuedVideoIndex}");
        }

        QueuedVideo deletedQueuedVideo = _roomManager.DeleteQueuedVideo(roomHash, queuedVideoIndex);

        if (deletedQueuedVideo == null)
        {
            _logger.LogInformation($"{roomHash} DeleteQueuedVideo: Error when deleting a queued video. Authorization Token: {authorizationToken}, QueuedVideoIndex: {queuedVideoIndex}");
        }

    
        _logger.LogInformation($"{roomHash} DeleteQueuedVideo: {deletedQueuedVideo.Url}. Authorization Token: {authorizationToken}, QueuedVideoIndex: {queuedVideoIndex}");

        await Clients.Group(roomHash).SendAsync(HubEvents.OnDeleteQueuedVideo, queuedVideoIndex);
    }
}