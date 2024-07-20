using DotnetServer.Api.HttpClasses.Input;
using DotnetServer.Core.Entities;

namespace DotnetServer.Api.Handlers;

public interface IRoomControllerHandler
{
    Room CreateRoom(RoomCreateInput input);
}