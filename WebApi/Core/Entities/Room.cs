using WebApi.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Core.Entities;

public class Room
{
    [Key]
    public string Hash { get; set; }

    public ICollection<ChatMessage> ChatMessages { get; set; }
    public ICollection<PlaylistVideo> PlaylistVideos { get; set; }
    public ICollection<User> Users { get; set; }
    public RoomSettings RoomSettings { get; set; }
    public UserPermissions UserPermissions { get; set; }
    public VideoPlayer VideoPlayer { get; set; }

    public Room() { }

    public Room(string roomName, string roomPassword)
    {
        Hash = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8);
        ChatMessages = new List<ChatMessage>();
        PlaylistVideos = new List<PlaylistVideo>();
        Users = new List<User>();
        RoomSettings = new RoomSettings(roomName, roomPassword, roomPassword.Length == 0 ? RoomTypes.Public : RoomTypes.Private);
        UserPermissions = new UserPermissions();
        VideoPlayer = new VideoPlayer();
    }
}