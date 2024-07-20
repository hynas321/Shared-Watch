using DotnetServer.Api.DTO;
using DotnetServer.Core.Entities;

namespace DotnetServer.Infrastructure.Repositories;

public interface IRoomRepository
{
    bool AddRoom(Room room);
    Room DeleteRoom(string roomHash);
    Room GetRoom(string roomHash);
    List<Room> GetRooms();
    IEnumerable<RoomDTO> GetRoomsDTO();
}