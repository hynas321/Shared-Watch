public class RoomJoinOutput
{
    public string AuthorizationToken { get; set; }
    public List<ChatMessage> ChatMessages { get; set; }
    public List<QueuedVideo> QueuedVideos { get; set; }
    public List<UserDTO> Users { get; set; }
    public RoomSettings RoomSettings { get; set; }
    public VideoPlayerSettings VideoPlayerSettings { get; set; }
}