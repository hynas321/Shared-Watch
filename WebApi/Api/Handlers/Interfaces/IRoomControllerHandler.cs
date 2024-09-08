using WebApi.Api.HttpClasses.Input;
using WebApi.Core.Entities;

namespace WebApi.Api.Handlers;

public interface IRoomControllerHandler
{
    Task<Room> CreateRoom(RoomCreateInput input);
}