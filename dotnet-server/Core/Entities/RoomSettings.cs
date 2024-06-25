public class RoomSettings : IRoomSettings
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