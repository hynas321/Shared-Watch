namespace dotnet_server.Api.Handlers.Interfaces
{
    public interface IRoomControllerHandler
    {
        Room CreateRoom(RoomCreateInput input);
    }
}
