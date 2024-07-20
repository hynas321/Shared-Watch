using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.KickOut)]
    public async Task KickOut(string roomHash, string authorizationToken, string usernameToKickOut)
    {
        try
        {
            Room room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = _userRepository.GetUserByAuthorizationToken(authorizationToken);

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

            User userToKickOut = _userRepository.GetUserByUsername(roomHash, usernameToKickOut);

            if (userToKickOut == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: User does not exist {usernameToKickOut}. Authorization Token: {authorizationToken}");
                return;
            }

            _logger.LogInformation(
                $"{roomHash} KickOut: {userToKickOut.Username} {userToKickOut.IsAdmin} {userToKickOut.AuthorizationToken}. Authorization Token: {authorizationToken}"
            );

            User kickedOutUser = _userRepository.DeleteUser(roomHash, userToKickOut.AuthorizationToken);

            if (kickedOutUser == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: Error when kicking out a user. Authorization Token: {authorizationToken}");
                return;
            }

            UserDTO kickedOutUserDTO = new UserDTO(kickedOutUser.Username, kickedOutUser.IsAdmin);

            await Clients.Group(roomHash).SendAsync(HubMessages.OnKickOut, JsonHelper.Serialize(kickedOutUserDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    [HubMethodName(HubMessages.SetAdminStatus)]
    public async Task SetAdminStatus(string roomHash, string authorizationToken, string usernameToSetAdminStatus, bool isAdmin)
    {
        try
        {
            Room room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = _userRepository.GetUserByAuthorizationToken(authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (user.IsAdmin == false)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: User does not have the permission. Authorization Token: {authorizationToken}");
                return;
            }

            User updatedUser = _userRepository.GetUserByUsername(roomHash, usernameToSetAdminStatus);

            if (updatedUser == null)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: User does not exist {usernameToSetAdminStatus}. Authorization Token: {authorizationToken}");
                return;
            }

            updatedUser.IsAdmin = isAdmin;

            if (isAdmin)
            {
                room.AdminTokens.Add(updatedUser.AuthorizationToken);
            }
            else
            {
                room.AdminTokens = room.AdminTokens.Where(x => x != updatedUser.AuthorizationToken).ToList();
            }

            UserDTO updatedUserDTO = new UserDTO(updatedUser.Username, updatedUser.IsAdmin);

            _logger.LogInformation($"{roomHash} SetAdminStatus: {updatedUser.Username} {updatedUser.IsAdmin} {updatedUser.AuthorizationToken}. Authorization Token: {authorizationToken}");

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetAdminStatus, JsonHelper.Serialize(updatedUserDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}