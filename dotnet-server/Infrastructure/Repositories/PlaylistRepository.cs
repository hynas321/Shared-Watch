public class PlaylistRepository : IPlaylistRepository
{
    private readonly IRoomRepository _roomRepository;

    public PlaylistRepository(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public bool AddPlaylistVideo(string roomHash, IPlaylistVideo playlistVideo)
    {
        IRoom room = _roomRepository.GetRoom(roomHash);

        if (room == null) return false;

        room.PlaylistVideos = room.PlaylistVideos.Concat([playlistVideo]);

        return true;
    }

    public IPlaylistVideo DeletePlaylistVideo(string roomHash, string videoHash)
    {
        IRoom room = _roomRepository.GetRoom(roomHash);

        if (room == null) return null;

        IPlaylistVideo playlistVideo = room.PlaylistVideos.FirstOrDefault(v => v.Hash == videoHash);

        if (playlistVideo == null) return null;

        room.PlaylistVideos = room.PlaylistVideos.Where(v => v.Hash != videoHash);

        return playlistVideo;
    }
}