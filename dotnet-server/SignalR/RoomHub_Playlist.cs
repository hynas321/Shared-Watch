using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.AddPlaylistVideo)]
    public async Task AddPlaylistVideo(string roomHash, string authorizationToken, PlaylistVideo playlistVideo)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false && room.UserPermissions.canAddVideo == false)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: User does not have permission. Authorization Token: {authorizationToken}");
            return;
        }

        _logger.LogInformation($"{roomHash} AddPlaylistVideo: {playlistVideo.Url}. Authorization Token: {authorizationToken}");

        bool isPlaylistVideoAdded = _roomManager.AddPlaylistVideo(roomHash, playlistVideo);

        if (!isPlaylistVideoAdded)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: Error when adding a queued video. Authorization Token: {authorizationToken}");
            return;
        }

        _logger.LogInformation(room.PlaylistVideos.Count.ToString());
        if (room.PlaylistVideos.Count == 1 && _playlistHandler.IsHandlerRunning == false)
        {
            _playlistHandler.StartPlaylistHandler(roomHash);
        }

        await Clients.Group(roomHash).SendAsync(HubEvents.OnAddPlaylistVideo, JsonHelper.Serialize(playlistVideo));
    }

    [HubMethodName(HubEvents.DeletePlaylistVideo)]
    public async Task DeletePlaylistVideo(string roomHash, string authorizationToken, int playlistVideoIndex)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Room does not exist. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {playlistVideoIndex}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: User does not exist. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {playlistVideoIndex}");
            return;
        }

        if (user.IsAdmin == false && room.UserPermissions.canRemoveVideo == false)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: User does not have the permission. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {playlistVideoIndex}");
        }

        PlaylistVideo deletePlaylistVideo = _roomManager.DeletePlaylistVideo(roomHash, playlistVideoIndex);

        if (deletePlaylistVideo == null)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Error when deleting a queued video. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {playlistVideoIndex}");
        }

        _logger.LogInformation($"{roomHash} DeletePlaylistVideo: {deletePlaylistVideo.Url}. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {playlistVideoIndex}");

        await Clients.Group(roomHash).SendAsync(HubEvents.OnDeletePlaylistVideo, playlistVideoIndex);
    }
}