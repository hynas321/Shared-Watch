using WebApi.Core.Enums;

namespace WebApi.Api.DTO;

public class RoomDTO
{
    public string RoomHash { get; set; }
    public string RoomName { get; set; }
    public RoomTypes RoomType { get; set; }
    public int OccupiedSlots { get; set; }
    public int TotalSlots { get; set; }

    public RoomDTO() { }

    public RoomDTO(
        string roomHash,
        string roomName,
        RoomTypes roomType,
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
