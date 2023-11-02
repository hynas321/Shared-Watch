public class RoomDTO
{
    public string RoomHash { get; set; }
    public string RoomName { get; set; }
    public RoomTypesEnum RoomType { get; set; }
    public int OccupiedSlots { get; set; }
    public int TotalSlots { get; set; }

    public RoomDTO(
        string roomHash,
        string roomName,
        RoomTypesEnum roomType,
        int occupiedSlots,
        int totalSlots
    )
    {
        RoomHash = roomHash;
        RoomName = roomName;
        RoomType = roomType;
        OccupiedSlots = occupiedSlots;
        TotalSlots = totalSlots;
    }
}
