public class PlaylistRepository : IPlaylistRepository
{
    private readonly IRoomRepository _roomRepository;

    public PlaylistRepository(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public bool AddPlaylistVideo(string roomHash, PlaylistVideo playlistVideo)
    {
        Room room = _roomRepository.GetRoom(roomHash);

        if (room == null)
        {
            return false;
        }

        room.PlaylistVideos.Add(playlistVideo);
        return true;
    }

    public PlaylistVideo DeletePlaylistVideo(string roomHash, string videoHash)
    {
        Room room = _roomRepository.GetRoom(roomHash);

        if (room == null)
        {
            return null;
        }

        PlaylistVideo playlistVideo = room.PlaylistVideos.FirstOrDefault(v => v.Hash == videoHash);

        if (playlistVideo == null)
        {
            return null;
        }

        room.PlaylistVideos.Remove(playlistVideo);
        return playlistVideo;
    }
}