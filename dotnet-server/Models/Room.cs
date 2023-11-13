public class Room
{
    public string RoomHash { get; set; }
    public string RoomName { get; set; }
    public string RoomPassword { get; set; }
    public List<ChatMessage> ChatMessages { get; set; }
    public List<QueuedVideo> QueuedVideos { get; set; }
    public List<User> Users { get; set; }
    public RoomSettings RoomSettings { get; set; }
    public VideoPlayerSettings VideoPlayerSettings { get; set; }

    public Room(string roomName, string roomPassword)
    {
        RoomHash = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        RoomName = roomName;
        RoomPassword = roomPassword;
        ChatMessages = new List<ChatMessage>();
        QueuedVideos = new List<QueuedVideo>();
        Users = new List<User>();
        RoomSettings = new RoomSettings();
        VideoPlayerSettings = new VideoPlayerSettings();
    }
}

public enum RoomTypesEnum
{
    Public = 0,
    Private = 1
}