using DotnetServer.Core.Enums;

namespace DotnetServer.Core.Entities;

public class RoomSettings
{
    public string RoomName { get; set; }
    public string RoomPassword { get; set; }
    public RoomTypes RoomType { get; set; }
    public int MaxUsers { get; set; }

    public RoomSettings(string roomName, string roomPassword, RoomTypes roomType)
    {
        RoomName = roomName;
        RoomPassword = roomPassword;
        RoomType = roomType;
        MaxUsers = 10;
    }
}