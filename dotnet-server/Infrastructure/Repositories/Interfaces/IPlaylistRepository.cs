using DotnetServer.Core.Entities;

namespace DotnetServer.Infrastructure.Repositories;

public interface IPlaylistRepository
{
    bool AddPlaylistVideo(string roomHash, PlaylistVideo playlistVideo);
    PlaylistVideo DeletePlaylistVideo(string roomHash, string videoHash);
}