public class RoomSettings
{
    public string RoomName { get; set; }
    public string RoomPassword { get; set; }
    public RoomTypesEnum RoomType { get; set; }
    public int MaxUsers { get; set; }

    public RoomSettings(string roomName, string roomPassword, RoomTypesEnum roomType)
    {
        RoomName = roomName;
        RoomPassword = roomPassword;
        RoomType = roomType;
        MaxUsers = 6;
    }

    public RoomSettings(string roomName, string roomPassword, int maxUsers)
    {
        RoomName = roomName;
        RoomPassword = roomPassword;
        MaxUsers = maxUsers;
    }
}