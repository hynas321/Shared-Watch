using dotnet_server.Api.Handlers.Interfaces;

namespace dotnet_server.Api.Handlers
{
    public class RoomControllerHandler(IRoomRepository roomRepository) : IRoomControllerHandler
    {
        public Room CreateRoom(RoomCreateInput input)
        {
            Room room = new Room(input.RoomName, input.RoomPassword);

            roomRepository.AddRoom(room);

            return room;
        }
    }
}
