using WebApi.Core.Entities;
using WebApi.Core.Enums;
using Microsoft.AspNetCore.SignalR;
using WebApi.Application.Constants;
using System.Security.Claims;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.SetRoomPassword)]
    public async Task SetRoomPassword(string roomHash, string newRoomPassword)
    {
        var userRoleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRoleClaim != Role.Admin)
        {
            _logger.LogInformation($"{roomHash} SetRoomPassword: User is not authorized. User identifier: {Context.UserIdentifier}");
            return;
        }

        var room = await _roomRepository.GetRoomAsync(roomHash, Context.ConnectionAborted);

        if (room is null)
        {
            _logger.LogInformation($"{roomHash} SetRoomPassword: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        room.RoomSettings.RoomPassword = newRoomPassword;
        room.RoomSettings.RoomType = string.IsNullOrEmpty(newRoomPassword) ? RoomTypes.Public : RoomTypes.Private;

        await _roomRepository.UpdateRoomAsync(room, Context.ConnectionAborted);
        await Clients.Group(roomHash).SendAsync(HubMessages.OnSetRoomPassword, newRoomPassword, room.RoomSettings.RoomType, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetUserPermissions)]
    public async Task SetUserPermissions(string roomHash, UserPermissions userPermissions)
    {
        var userRoleClaim = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (userRoleClaim != Role.Admin)
        {
            _logger.LogInformation($"{roomHash} SetUserPermissions: User is not authorized. User identifier: {Context.UserIdentifier}");
            return;
        }

        var room = await _roomRepository.GetRoomAsync(roomHash, Context.ConnectionAborted);
        if (room is null)
        {
            _logger.LogInformation($"{roomHash} SetUserPermissions: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        room.UserPermissions = userPermissions;

        await _roomRepository.UpdateRoomAsync(room, Context.ConnectionAborted);
        await Clients.Group(roomHash).SendAsync(HubMessages.OnSetUserPermissions, userPermissions, Context.ConnectionAborted);
    }
}