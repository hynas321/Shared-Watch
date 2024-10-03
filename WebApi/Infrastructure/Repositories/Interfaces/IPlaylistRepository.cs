using WebApi.Core.Entities;

namespace WebApi.Infrastructure.Repositories;

public interface IPlaylistRepository
{
    Task<bool> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo);
    Task<PlaylistVideo> DeletePlaylistVideoAsync(string roomHash, string videoHash);
    Task<PlaylistVideo> GetPlaylistVideoAsync(string roomHash, string videoHash);
}