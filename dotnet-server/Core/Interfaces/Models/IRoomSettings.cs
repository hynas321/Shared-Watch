public interface IRoomSettings
{
    string RoomName { get; set; }
    string RoomPassword { get; set; }
    RoomTypes RoomType { get; set; }
    int MaxUsers { get; set; }
}