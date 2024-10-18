using System.Text.RegularExpressions;
using WebApi.Core.Entities;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Application.Constants;
using System.Security.Claims;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    const int maxPlaylistVideos = 10;

    [Authorize]
    [HubMethodName(HubMessages.AddPlaylistVideo)]
    public async Task AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo)
    {
        try
        {
            var room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Room does not exist. User identifier: {Context.UserIdentifier}");
                return;
            }

            if (room.PlaylistVideos.Count >= maxPlaylistVideos)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Playlist video limit reached. User identifier: {Context.UserIdentifier}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnAddPlaylistVideo, null);
                return;
            }

            var IsUrlCorrect = CheckIfIsYouTubeVideoLink(playlistVideo.Url);

            if (!IsUrlCorrect)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Url {playlistVideo.Url} is incorrect. User identifier: {Context.UserIdentifier}");
                return;
            }

            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (role != Role.Admin && !room.UserPermissions.CanAddVideo)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: User does not have permission. User identifier: {Context.UserIdentifier}");
                return;
            }

            var title = await _youtubeAPIService.GetVideoTitleAsync(playlistVideo.Url);

            if (title == null)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Could not find title {playlistVideo.Url}. User identifier: {Context.UserIdentifier}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnAddPlaylistVideo, null);
                return;
            }

            playlistVideo.Title = title;

            var thumbnailUrl = await _youtubeAPIService.GetVideoThumbnailUrlAsync(playlistVideo.Url);

            if (thumbnailUrl == null)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Could not find thumbnail URL {playlistVideo.Url}. User identifier: {Context.UserIdentifier}");
                await Clients.Client(Context.ConnectionId).SendAsync(HubMessages.OnAddPlaylistVideo, null);
                return;
            }

            playlistVideo.ThumbnailUrl = thumbnailUrl;
            playlistVideo.Hash = Guid.NewGuid().ToString();

            var isPlaylistVideoAdded = await _playlistRepository.AddPlaylistVideoAsync(roomHash, playlistVideo);

            if (!isPlaylistVideoAdded)
            {
                _logger.LogInformation($"{roomHash} AddPlaylistVideo: Error when adding a video {playlistVideo.Url}. User identifier: {Context.UserIdentifier}");
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

    [Authorize]
    [HubMethodName(HubMessages.DeletePlaylistVideo)]
    public async Task DeletePlaylistVideoAsync(string roomHash, string videoHash)
    {
        try
        {
            var room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Room does not exist. User identifier: {Context.UserIdentifier}");
                return;
            }

            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (role != Role.Admin && !room.UserPermissions.CanRemoveVideo)
            {
                _logger.LogInformation($"{roomHash} DeletePlaylistVideo: User does not have permission. User identifier: {Context.UserIdentifier}");
                return;
            }

            var deletedPlaylistVideo = await _playlistRepository.DeletePlaylistVideoAsync(roomHash, videoHash);

            if (deletedPlaylistVideo == null)
            {
                _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Error when deleting a queued video. User identifier: {Context.UserIdentifier}, PlaylistVideoHash: {videoHash}");
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
        var pattern = @"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:[^\/\n\s]+\/\S+\/|(?:v|e(?:mbed)?)\/|\S*?[?&]v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})";

        var regex = new Regex(pattern);
        var match = regex.Match(url);

        return match.Success;
    }
}