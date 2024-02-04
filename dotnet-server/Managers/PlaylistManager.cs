public class PlaylistManager {
    private readonly RoomManager _roomManager = new RoomManager();

    public bool AddPlaylistVideo(string roomHash, PlaylistVideo playlistVideo)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

            if (room == null)
            {
                return false;
            }

            playlistVideo.Hash = Guid.NewGuid().ToString().Replace("-", "")[..8];;
            room.PlaylistVideos.Add(playlistVideo);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public PlaylistVideo DeletePlaylistVideo(string roomHash, string videoHash)
    {
        try
        {
            Room room = _roomManager.GetRoom(roomHash);

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
        catch (Exception)
        {
            return null;
        }
    }
}