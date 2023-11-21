using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.KickOut)]
    public async Task KickOut(string roomHash, string authorizationToken, string usernameToKickOut)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} KickOut: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} KickOut: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false)
        {
            _logger.LogInformation($"{roomHash} KickOut: User does not have the permission. Authorization Token: {authorizationToken}");
            return;
        }

        User userToKickOut = _roomManager.GetUserByUsername(roomHash, usernameToKickOut);

        if (userToKickOut == null)
        {
            _logger.LogInformation($"{roomHash} KickOut: User does not exist {usernameToKickOut}. Authorization Token: {authorizationToken}");
            return;
        }

        _logger.LogInformation($"{roomHash} KickOut: {userToKickOut.Username} {userToKickOut.IsAdmin} {userToKickOut.AuthorizationToken}. Authorization Token: {authorizationToken}");

        User kickedOutUser = _roomManager.DeleteUser(roomHash, userToKickOut.AuthorizationToken);

        if (kickedOutUser == null)
        {
            _logger.LogInformation($"{roomHash} KickOut: Error when kicking out a user. Authorization Token: {authorizationToken}");
            return;
        }

        UserDTO kickedOutUserDTO = new UserDTO(kickedOutUser.Username, kickedOutUser.IsAdmin);

        await Clients.Group(roomHash).SendAsync(HubEvents.OnKickOut, JsonHelper.Serialize(kickedOutUserDTO));
    }
}