using System.Text.RegularExpressions;
using WebApi.Core.Entities;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.AddPlaylistVideo)]
    public async Task AddPlaylistVideoAsync(string roomHash, string authorizationToken, PlaylistVideo playlistVideo)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            bool IsUrlCorrect = CheckIfIsYouTubeVideoLink(playlistVideo.Url);

            if (!IsUrlCorrect)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Url {playlistVideo.Url} is incorrect. Authorization Token: {authorizationToken}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnAddPlaylistVideo, null);
                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin && !room.UserPermissions.CanAddVideo)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            string title = await _youtubeAPIService.GetVideoTitleAsync(playlistVideo.Url);

            if (title == null)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Could not find title {playlistVideo.Url}. Authorization Token: {authorizationToken}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnAddPlaylistVideo, null);
                return;
            }

            playlistVideo.Title = title;

            string thumbnailUrl = await _youtubeAPIService.GetVideoThumbnailUrlAsync(playlistVideo.Url);

            if (thumbnailUrl == null)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Could not find thumbnail URL {playlistVideo.Url}. Authorization Token: {authorizationToken}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnAddPlaylistVideo, null);
                return;
            }

            playlistVideo.ThumbnailUrl = thumbnailUrl;
            playlistVideo.Hash = Guid.NewGuid().ToString();

            bool isPlaylistVideoAdded = await _playlistRepository.AddPlaylistVideoAsync(roomHash, playlistVideo);

            if (!isPlaylistVideoAdded)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Error when adding a video {playlistVideo.Url}. Authorization Token: {authorizationToken}");
                return;
            }

            if (room.PlaylistVideos.Count == 1 && !_playlistService.IsServiceRunning)
            {
                _playlistService.StartPlaylistService(roomHash);
            }

            await Clients.Group(roomHash).SendAsync(HubMessages.OnAddPlaylistVideo, JsonHelper.Serialize(playlistVideo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    [HubMethodName(HubMessages.DeletePlaylistVideo)]
    public async Task DeletePlaylistVideoAsync(string roomHash, string authorizationToken, string videoHash)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Room does not exist. Authorization Token: {authorizationToken}, PlaylistVideoHash: {videoHash}");
                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} DeletePlaylistVideo: User does not exist. Authorization Token: {authorizationToken}, PlaylistVideoHash: {videoHash}");
                return;
            }

            if (!user.IsAdmin && !room.UserPermissions.CanRemoveVideo)
            {
                _logger.LogInformation($"{roomHash} DeletePlaylistVideo: User does not have permission. Authorization Token: {authorizationToken}, PlaylistVideoHash: {videoHash}");
                return;
            }

            PlaylistVideo deletedPlaylistVideo = await _playlistRepository.DeletePlaylistVideoAsync(roomHash, videoHash);

            if (deletedPlaylistVideo == null)
            {
                _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Error when deleting a queued video. Authorization Token: {authorizationToken}, PlaylistVideoHash: {videoHash}");
                return;
            }

            await Clients.Group(roomHash).SendAsync(HubMessages.OnDeletePlaylistVideo, videoHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    private bool CheckIfIsYouTubeVideoLink(string url)
    {
        string pattern = @"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})";

        Regex regex = new Regex(pattern);
        Match match = regex.Match(url);

        return match.Success;
    }
}