using DotnetServer.Api.HttpClasses.Input;
using DotnetServer.Core.Entities;
using DotnetServer.Infrastructure.Repositories;

namespace DotnetServer.Api.Handlers;

public class RoomControllerHandler(IRoomRepository roomRepository) : IRoomControllerHandler
{
    public async Task<Room> CreateRoom(RoomCreateInput input)
    {
        Room room = new Room(input.RoomName, input.RoomPassword);

        await roomRepository.AddRoomAsync(room);

        return room;
    }
}
