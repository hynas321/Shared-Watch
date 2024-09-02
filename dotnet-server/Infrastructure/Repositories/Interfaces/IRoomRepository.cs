using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;

namespace DotnetServer.Infrastructure.Repositories;

public interface IRoomRepository
{
    Task<bool> AddRoomAsync(Room room);
    Task<Room> DeleteRoomAsync(string roomHash);
    Task<bool> UpdateRoomAsync(Room room);
    Task<Room> GetRoomAsync(string roomHash);
    Task<List<Room>> GetRoomsAsync();
    Task<IEnumerable<RoomDTO>> GetRoomsDTOAsync();
}