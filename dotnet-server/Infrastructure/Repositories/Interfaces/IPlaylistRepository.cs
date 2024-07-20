public interface IPlaylistRepository
{
    bool AddPlaylistVideo(string roomHash, PlaylistVideo playlistVideo);
    PlaylistVideo DeletePlaylistVideo(string roomHash, string videoHash);
}