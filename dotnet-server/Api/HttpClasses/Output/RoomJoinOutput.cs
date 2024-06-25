public class RoomJoinOutput
{
    public string AuthorizationToken { get; set; }
    public bool IsAdmin { get; set; }

    public List<ChatMessage> ChatMessages { get; set; }
    public List<PlaylistVideo> PlaylistVideos { get; set; }
    public List<UserDTO> Users { get; set; }
    public RoomSettings RoomSettings { get; set; }
    public UserPermissions UserPermissions { get; set; }
    public VideoPlayerState VideoPlayerState { get; set; }
}