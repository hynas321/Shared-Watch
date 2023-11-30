using System.Text.RegularExpressions;
using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.AddPlaylistVideo)]
    [Obsolete]
    public async Task AddPlaylistVideo(string roomHash, string authorizationToken, PlaylistVideo playlistVideo)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        bool IsUrlCorrect = CheckIfIsYouTubeVideoLink(playlistVideo.Url);

        if (!IsUrlCorrect)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: Url {playlistVideo.Url} is incorrect. Authorization Token: {authorizationToken}");
            await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnAddPlaylistVideo, null);
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

        string title = _youtubeAPIService.GetVideoTitle(playlistVideo.Url);

        if (title == null)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: Could not find title {playlistVideo.Url}. Authorization Token: {authorizationToken}");
            await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnAddPlaylistVideo, null);
            return;
        }

        playlistVideo.Title = title;

        string thumbnailUrl = _youtubeAPIService.GetVideoThumbnailUrl(playlistVideo.Url);

        if (thumbnailUrl == null)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: Could not find thumbnail URL {playlistVideo.Url}. Authorization Token: {authorizationToken}");
            await Clients.Client(Context.ConnectionId).SendAsync(HubEvents.OnAddPlaylistVideo, null);
            return;
        }
        
        playlistVideo.ThumbnailUrl = thumbnailUrl;

        bool isPlaylistVideoAdded = _roomManager.AddPlaylistVideo(roomHash, playlistVideo);

        if (!isPlaylistVideoAdded)
        {
            _logger.LogInformation($"{roomHash} AddPlaylistVideo: Error when adding a video {playlistVideo.Url}. Authorization Token: {authorizationToken}");
            return;
        }

        if (room.PlaylistVideos.Count == 1 && _playlistHandler.IsHandlerRunning == false)
        {
            _playlistHandler.StartPlaylistHandler(roomHash);
        }

        await Clients.Group(roomHash).SendAsync(HubEvents.OnAddPlaylistVideo, JsonHelper.Serialize(playlistVideo));
    }

    [HubMethodName(HubEvents.DeletePlaylistVideo)]
    public async Task DeletePlaylistVideo(string roomHash, string authorizationToken, string videoHash)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Room does not exist. Authorization Token: {authorizationToken}, PlaylistVideoHash: {videoHash}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: User does not exist. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {videoHash}");
            return;
        }

        if (user.IsAdmin == false && room.UserPermissions.canRemoveVideo == false)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: User does not have the permission. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {videoHash}");
        }

        PlaylistVideo deletePlaylistVideo = _roomManager.DeletePlaylistVideo(roomHash, videoHash);

        if (deletePlaylistVideo == null)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Error when deleting a queued video. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {videoHash}");
        }

        _logger.LogInformation($"{roomHash} DeletePlaylistVideo: {deletePlaylistVideo.Url}. Authorization Token: {authorizationToken}, PlaylistVideoIndex: {videoHash}");

        await Clients.Group(roomHash).SendAsync(HubEvents.OnDeletePlaylistVideo, videoHash);
    }

    private bool CheckIfIsYouTubeVideoLink(string url)
    {
        string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})";
        Regex regex = new Regex(pattern);

        Match match = regex.Match(url);

        return match.Success;
    }
}