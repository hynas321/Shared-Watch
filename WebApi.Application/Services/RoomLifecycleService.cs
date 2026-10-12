using WebApi.Application.Repositories;
using WebApi.Application.Services;

namespace WebApi.Application.Services;

public sealed class RoomLifecycleService : IRoomLifecycleService
{
    private readonly IRoomRepository _roomRepository;

    public RoomLifecycleService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<bool> DeleteIfEmptyAsync(string roomHash, CancellationToken cancellationToken)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);
        if (room is null || room.Users.Count != 0)
            return false;

        return await _roomRepository.DeleteRoomAsync(roomHash, cancellationToken) is not null;
    }
}

