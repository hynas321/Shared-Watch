using DotnetServer.Core.Entities;

namespace DotnetServer.Infrastructure.Repositories;

public interface IPlaylistRepository
{
    Task<bool> AddPlaylistVideoAsync(string roomHash, PlaylistVideo playlistVideo);
    Task<PlaylistVideo> DeletePlaylistVideoAsync(string roomHash, string videoHash);
}