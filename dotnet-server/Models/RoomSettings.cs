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
        MaxUsers = 10;
    }
}