public class Room
{
    public string RoomHash { get; set; }
    public string RoomName { get; set; }
    public string RoomPassword { get; set; }
    public List<ChatMessage> ChatMessages { get; set; }
    public List<PlaylistVideo> PlaylistVideos { get; set; }
    public List<User> Users { get; set; }
    public UserPermissions UserPermissions { get; set; }
    public VideoPlayerState VideoPlayerState { get; set; }

    public Room(string roomName, string roomPassword)
    {
        RoomHash = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        RoomName = roomName;
        RoomPassword = roomPassword;
        ChatMessages = new List<ChatMessage>();
        PlaylistVideos = new List<PlaylistVideo>();
        Users = new List<User>();
        UserPermissions = new UserPermissions();
        VideoPlayerState = new VideoPlayerState();
    }

    public User GetUser(string authorizationToken)
    {
        return Users.FirstOrDefault(u => u.AuthorizationToken == authorizationToken);
    }
}

public enum RoomTypesEnum
{
    Public = 0,
    Private = 1
}