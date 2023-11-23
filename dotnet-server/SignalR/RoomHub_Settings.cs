using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.SetRoomPassword)]
    public async Task SetRoomPassword(string roomHash, string authorizationToken, string newRoomPassword)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} SetRoomPassword: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} SetRoomPassword: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false)
        {
            _logger.LogInformation($"{roomHash} SetRoomPassword: User does not have the permission. Authorization Token: {authorizationToken}");
            return;
        }

        room.RoomSettings.RoomPassword = newRoomPassword;
        room.RoomSettings.RoomType = newRoomPassword.Length == 0 ? RoomTypesEnum.Public : RoomTypesEnum.Private;

        _logger.LogInformation($"{roomHash} SetRoomPassword: New room password set. Authorization Token: {authorizationToken}");

        await Clients.Group(roomHash).SendAsync(HubEvents.OnSetRoomPassword, newRoomPassword, room.RoomSettings.RoomType);
    }

    [HubMethodName(HubEvents.SetUserPermissions)]
    public async Task SetUserPermissions(string roomHash, string authorizationToken, UserPermissions userPermissions)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} SetUserPermissions: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} SetUserPermissions: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false)
        {
            _logger.LogInformation($"{roomHash} SetUserPermissions: User does not have the permission. Authorization Token: {authorizationToken}");
            return;
        }

        room.UserPermissions = userPermissions;

        _logger.LogInformation($"{roomHash} User permissions set. Authorization Token: {authorizationToken}");

        await Clients.Group(roomHash).SendAsync(HubEvents.OnSetUserPermissions, JsonHelper.Serialize(userPermissions));
    }
}