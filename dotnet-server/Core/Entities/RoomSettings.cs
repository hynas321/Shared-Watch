using DotnetServer.Core.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DotnetServer.Core.Entities;

public class RoomSettings
{
    [Key, ForeignKey("Room")]
    public string RoomHash { get; set; }

    [Required]
    public string RoomName { get; set; }
    public string RoomPassword { get; set; }
    public RoomTypes RoomType { get; set; }
    public int MaxUsers { get; set; }

    public RoomSettings() { }

    public RoomSettings(string roomName, string roomPassword, RoomTypes roomType)
    {
        RoomName = roomName;
        RoomPassword = roomPassword;
        RoomType = roomType;
        MaxUsers = 10;
    }
}