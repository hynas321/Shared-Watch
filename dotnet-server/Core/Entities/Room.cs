public class Room : IRoom
{
    public string RoomHash { get; set; }
    public IEnumerable<string> AdminTokens { get; set; }
    public IEnumerable<IChatMessage> ChatMessages { get; set; }
    public IEnumerable<IPlaylistVideo> PlaylistVideos { get; set; }
    public IEnumerable<IUser> Users { get; set; }
    public IRoomSettings RoomSettings { get; set; }
    public IUserPermissions UserPermissions { get; set; }
    public IVideoPlayerState VideoPlayerState { get; set; }

    public Room(string roomName, string roomPassword, RoomTypes roomType)
    {
        RoomHash = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        AdminTokens = new List<string>();
        ChatMessages = new List<IChatMessage>();
        PlaylistVideos = new List<IPlaylistVideo>();
        Users = new List<IUser>();
        RoomSettings = new RoomSettings(roomName, roomPassword, roomType);
        UserPermissions = new UserPermissions();
        VideoPlayerState = new VideoPlayerState();
    }
}