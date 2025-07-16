using WebApi.Core.Entities;
using WebApi.Core.Enums;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Application.Constants;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [Authorize(Roles = Role.Admin)]
    [HubMethodName(HubMessages.SetRoomPassword)]
    public async Task SetRoomPassword(string roomHash, string newRoomPassword)
    {
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

    [Authorize(Roles = Role.Admin)]
    [HubMethodName(HubMessages.SetUserPermissions)]
    public async Task SetUserPermissions(string roomHash, UserPermissions userPermissions)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, Context.ConnectionAborted);
        if (room is null)
        {
            _logger.LogInformation($"{roomHash} SetUserPermissions: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        room.UserPermissions = userPermissions;

        await _roomRepository.UpdateRoomAsync(room, Context.ConnectionAborted);
        await Clients.Group(roomHash).SendAsync(HubMessages.OnSetUserPermissions, JsonHelper.Serialize(userPermissions), Context.ConnectionAborted);
    }
}