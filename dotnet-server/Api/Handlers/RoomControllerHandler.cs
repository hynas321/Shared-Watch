using DotnetServer.Api.HttpClasses.Input;
using DotnetServer.Core.Entities;
using DotnetServer.Infrastructure.Repositories;

namespace DotnetServer.Api.Handlers;

public class RoomControllerHandler(IRoomRepository roomRepository) : IRoomControllerHandler
{
    public Room CreateRoom(RoomCreateInput input)
    {
        Room room = new Room(input.RoomName, input.RoomPassword);

        roomRepository.AddRoom(room);

        return room;
    }
}
