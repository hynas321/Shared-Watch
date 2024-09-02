using DotnetServer.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace DotnetServer.Core.Entities;

public class Room
{
    [Key]
    public string Hash { get; set; }

    public List<string> AdminTokens { get; set; }

    // One-to-Many relationship with ChatMessage
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    // One-to-Many relationship with PlaylistVideo
    public virtual ICollection<PlaylistVideo> PlaylistVideos { get; set; } = new List<PlaylistVideo>();

    // One-to-Many relationship with Users
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    // One-to-One relationships
    public RoomSettings RoomSettings { get; set; }
    public UserPermissions UserPermissions { get; set; }
    public VideoPlayer VideoPlayer { get; set; }

    public Room() { }

    public Room(string roomName, string roomPassword)
    {
        Hash = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        AdminTokens = new List<string>();
        RoomSettings = new RoomSettings(roomName, roomPassword, roomPassword.Length == 0 ? RoomTypes.Public : RoomTypes.Private);
        UserPermissions = new UserPermissions();
        VideoPlayer = new VideoPlayer();
    }
}