using WebApi.Application.Repositories;
using WebApi.Application.Services;
using WebApi.Core.Entities;

namespace WebApi.Application.Services;

public sealed class PlaylistApplicationService : IPlaylistApplicationService
{
    private readonly IRoomRepository _rooms;
    private readonly IPlaylistRepository _playlist;
    private readonly IYouTubeAPIService _youtube;
    private readonly IUploadedVideoStorage _storage;

    public PlaylistApplicationService(IRoomRepository rooms, IPlaylistRepository playlist, IYouTubeAPIService youtube, IUploadedVideoStorage storage)
    {
        _rooms = rooms;
        _playlist = playlist;
        _youtube = youtube;
        _storage = storage;
    }

    public async Task<AddYouTubeVideoResult> AddYouTubeVideoAsync(string roomHash, string? url, bool isAdmin, CancellationToken cancellationToken)
    {
        var room = await _rooms.GetRoomAsync(roomHash, cancellationToken);
        if (room is null || string.IsNullOrWhiteSpace(url))
            return new(AddYouTubeVideoStatus.VideoUnavailable);
        if (!isAdmin && !room.UserPermissions.CanAddVideo)
            return new(AddYouTubeVideoStatus.Forbidden);
        if (room.PlaylistVideos.Any(video => string.Equals(video.Url, url, StringComparison.OrdinalIgnoreCase)))
            return new(AddYouTubeVideoStatus.AlreadyExists);

        try
        {
            var duration = await _youtube.GetVideoDurationAsync(url);
            var title = await _youtube.GetVideoTitleAsync(url);
            var thumbnail = await _youtube.GetVideoThumbnailUrlAsync(url);
            if (duration <= 0 || string.IsNullOrWhiteSpace(title))
                return new(AddYouTubeVideoStatus.VideoUnavailable);

            var video = new PlaylistVideo(Guid.NewGuid().ToString("N"), url, duration, title, thumbnail ?? string.Empty)
            {
                RoomHash = roomHash
            };
            var addResult = await _playlist.AddPlaylistVideoAsync(roomHash, video, cancellationToken);
            return addResult switch
            {
                PlaylistVideoAddResult.Added => new(AddYouTubeVideoStatus.Added, video),
                PlaylistVideoAddResult.PlaylistFull => new(AddYouTubeVideoStatus.PlaylistFull),
                _ => new(AddYouTubeVideoStatus.VideoUnavailable)
            };
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new(AddYouTubeVideoStatus.VideoUnavailable);
        }
    }

    public async Task<DeletePlaylistVideoResult?> DeleteVideoAsync(string roomHash, string videoHash, bool isAdmin, CancellationToken cancellationToken)
    {
        var room = await _rooms.GetRoomAsync(roomHash, cancellationToken);
        if (room is null || (!isAdmin && !room.UserPermissions.CanRemoveVideo))
            return null;

        var video = await _playlist.DeletePlaylistVideoAsync(roomHash, videoHash, cancellationToken);
        if (video is null)
            return null;

        var cleanupFailed = false;
        if (video.Url?.StartsWith("/api/VideoFiles/", StringComparison.Ordinal) == true)
        {
            var fileName = Path.GetFileName(video.Url);
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                try { await _storage.DeleteAsync(fileName, cancellationToken); }
                catch (Exception exception) when (exception is not OperationCanceledException) { cleanupFailed = true; }
            }
        }

        return new DeletePlaylistVideoResult(video.Hash, cleanupFailed);
    }

    public async Task<IReadOnlyList<PlaylistVideo>?> ReorderVideosAsync(
        string roomHash,
        IReadOnlyList<string> videoHashes,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        if (!isAdmin || videoHashes.Count > 10 || videoHashes.Any(string.IsNullOrWhiteSpace))
        {
            return null;
        }

        if (!await _playlist.ReorderPlaylistVideosAsync(roomHash, videoHashes, cancellationToken))
        {
            return null;
        }

        var room = await _rooms.GetRoomAsync(roomHash, cancellationToken);
        return room?.PlaylistVideos.OrderBy(video => video.Position).ToList();
    }
}
