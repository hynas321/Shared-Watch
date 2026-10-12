using WebApi.Core.Entities;
namespace WebApi.Application.Repositories;
public enum PlaylistVideoAddResult { Added, RoomNotFound, PlaylistFull }
public interface IPlaylistRepository
{
    Task<PlaylistVideoAddResult> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo, CancellationToken cancellationToken);
    Task<PlaylistVideo?> DeletePlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken);
    Task<PlaylistVideo?> GetPlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken);
    Task<bool> RoomHasPlaylistVideoUrlAsync(string roomHash, string url, CancellationToken cancellationToken);
    Task<bool> ReorderPlaylistVideosAsync(string roomHash, IReadOnlyList<string> videoHashes, CancellationToken cancellationToken);
}
