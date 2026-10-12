using WebApi.Core.Entities;
using WebApi.Core.Enums;

namespace WebApi.Application.DTO;

public class RoomDTO
{
    public string? RoomHash { get; set; }
    public string? RoomName { get; set; }
    public RoomTypes RoomType { get; set; }
    public int OccupiedSlots { get; set; }
    public int TotalSlots { get; set; }

    public RoomDTO() { }

    public static RoomDTO FromEntity(Room room) => new(
        room.Hash ?? string.Empty,
        room.RoomSettings.RoomName ?? string.Empty,
        room.RoomSettings.RoomType,
        room.Users.Count,
        room.RoomSettings.MaxUsers);

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
