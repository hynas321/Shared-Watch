public class RoomJoinOutput
{
    public string AuthorizationToken { get; set; }
    public bool IsAdmin { get; set; }

    public string RoomName { get; set; }
    public List<ChatMessage> ChatMessages { get; set; }
    public List<PlaylistVideo> PlaylistVideos { get; set; }
    public List<UserDTO> Users { get; set; }
    public UserPermissions RoomSettings { get; set; }
    public VideoPlayerState VideoPlayerSettings { get; set; }
}