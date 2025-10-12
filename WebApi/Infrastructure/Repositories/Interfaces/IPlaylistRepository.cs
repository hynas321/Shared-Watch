using WebApi.Core.Entities;

namespace WebApi.Infrastructure.Repositories;

public interface IPlaylistRepository
{
    Task<bool> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVide, CancellationToken cancellationToken);
    Task<PlaylistVideo?> DeletePlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken);
    Task<PlaylistVideo?> GetPlaylistVideoAsync(string roomHash, string videoHash, CancellationToken cancellationToken);
}