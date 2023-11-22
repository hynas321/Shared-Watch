using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
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