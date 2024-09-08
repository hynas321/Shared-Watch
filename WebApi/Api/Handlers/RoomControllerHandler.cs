using WebApi.Api.HttpClasses.Input;
using WebApi.Core.Entities;
using WebApi.Infrastructure.Repositories;

namespace WebApi.Api.Handlers;

public class RoomControllerHandler(IRoomRepository roomRepository) : IRoomControllerHandler
{
    public async Task<Room> CreateRoom(RoomCreateInput input)
    {
        Room room = new Room(input.RoomName, input.RoomPassword);

        await roomRepository.AddRoomAsync(room);

        return room;
    }
}
