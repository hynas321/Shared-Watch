using WebApi.Core.Entities;
namespace WebApi.Application.Services;
public enum AddYouTubeVideoStatus { Added, PlaylistFull, VideoUnavailable, Forbidden, AlreadyExists }
public sealed record AddYouTubeVideoResult(AddYouTubeVideoStatus Status, PlaylistVideo? Video = null);
public sealed record DeletePlaylistVideoResult(string? Hash, bool UploadedFileCleanupFailed);
public interface IPlaylistApplicationService
{
    Task<AddYouTubeVideoResult> AddYouTubeVideoAsync(string roomHash, string? url, bool isAdmin, CancellationToken cancellationToken);
    Task<DeletePlaylistVideoResult?> DeleteVideoAsync(string roomHash, string videoHash, bool isAdmin, CancellationToken cancellationToken);
    Task<IReadOnlyList<PlaylistVideo>?> ReorderVideosAsync(string roomHash, IReadOnlyList<string> videoHashes, bool isAdmin, CancellationToken cancellationToken);
}
