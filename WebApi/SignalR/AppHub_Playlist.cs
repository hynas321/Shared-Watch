using WebApi.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApi.Application.Constants;
using WebApi.Application.Services;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [Authorize]
    [HubMethodName(HubMessages.AddPlaylistVideo)]
    public async Task<object> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo)
    {
        var isAdmin = await IsCurrentUserAdminAsync(roomHash, Context.ConnectionAborted);
        var result = await _playlistApplicationService.AddYouTubeVideoAsync(
            roomHash,
            playlistVideo.Url,
            isAdmin,
            Context.ConnectionAborted);

        if (result.Status == AddYouTubeVideoStatus.PlaylistFull ||
            result.Status == AddYouTubeVideoStatus.VideoUnavailable)
        {
            await Clients.Client(Context.ConnectionId)
                .SendAsync(HubMessages.OnAddPlaylistVideo, null, Context.ConnectionAborted);
        }

        if (result.Status != AddYouTubeVideoStatus.Added || result.Video is null)
        {
            _logger.LogInformation(
                "{RoomHash} AddPlaylistVideo failed with {Status}. User identifier: {UserIdentifier}",
                roomHash,
                result.Status,
                Context.UserIdentifier);
            return result.Status.ToString();
        }

        _playlistService.StartPlaylistService(roomHash);

        await Clients.Group(roomHash)
            .SendAsync(HubMessages.OnAddPlaylistVideo, result.Video, Context.ConnectionAborted);
        return result.Video;
    }

    [Authorize]
    [HubMethodName(HubMessages.DeletePlaylistVideo)]
    public async Task DeletePlaylistVideoAsync(string roomHash, string videoHash)
    {
        var isAdmin = await IsCurrentUserAdminAsync(roomHash, Context.ConnectionAborted);
        var deletedPlaylistVideo = await _playlistApplicationService.DeleteVideoAsync(
            roomHash,
            videoHash,
            isAdmin,
            Context.ConnectionAborted);

        if (deletedPlaylistVideo is null)
        {
            _logger.LogInformation($"{roomHash} DeletePlaylistVideo: Error when deleting a queued video. User identifier: {Context.UserIdentifier}, PlaylistVideoHash: {videoHash}");
            return;
        }

        if (deletedPlaylistVideo.UploadedFileCleanupFailed)
        {
            _logger.LogWarning(
                "Could not remove uploaded video file for room {RoomHash}, video {VideoHash}.",
                roomHash,
                videoHash);
        }

        await Clients.Group(roomHash).SendAsync(HubMessages.OnDeletePlaylistVideo, videoHash, Context.ConnectionAborted);
    }

    [Authorize]
    [HubMethodName(HubMessages.ReorderPlaylistVideos)]
    public async Task ReorderPlaylistVideosAsync(string roomHash, string[] videoHashes)
    {
        var isAdmin = await IsCurrentUserAdminAsync(roomHash, Context.ConnectionAborted);
        var reorderedVideos = await _playlistApplicationService.ReorderVideosAsync(
            roomHash,
            videoHashes,
            isAdmin,
            Context.ConnectionAborted);

        if (reorderedVideos is null)
        {
            return;
        }

        await Clients.Group(roomHash).SendAsync(
            HubMessages.OnPlaylistReordered,
            reorderedVideos,
            Context.ConnectionAborted);
    }
}
