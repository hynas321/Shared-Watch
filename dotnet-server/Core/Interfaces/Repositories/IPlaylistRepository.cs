public interface IPlaylistRepository
{
    bool AddPlaylistVideo(string roomHash, IPlaylistVideo playlistVideo);
    IPlaylistVideo DeletePlaylistVideo(string roomHash, string videoHash);
}