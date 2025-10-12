using WebApi.Api.DTO;
using WebApi.Core.Entities;

namespace WebApi.Infrastructure.Repositories;

public interface IRoomRepository
{
    Task<bool> AddRoomAsync(Room room, CancellationToken cancellationToken);
    Task<Room?> DeleteRoomAsync(string roomHash, CancellationToken cancellationToken);
    Task<bool> UpdateRoomAsync(Room room, CancellationToken cancellationToken);
    Task<Room?> GetRoomAsync(string roomHash, CancellationToken cancellationToken);
    Task<Room?> GetRoomByNameAsync(string roomName, CancellationToken cancellationToken);
    Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken);
    Task<IEnumerable<RoomDTO>> GetRoomsDTOAsync(CancellationToken cancellationToken);
}

