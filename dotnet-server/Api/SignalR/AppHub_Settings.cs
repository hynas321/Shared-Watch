using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.SetRoomPassword)]
    public async Task SetRoomPassword(string roomHash, string authorizationToken, string newRoomPassword)
    {
        try
        {
            Room room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetRoomPassword: Room does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetRoomPassword: User does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            if (user.IsAdmin == false)
            {
                _logger.LogInformation(
                    $"{roomHash} SetRoomPassword: User does not have the permission. Authorization Token: {authorizationToken}"
                );

                return;
            }

            room.RoomSettings.RoomPassword = newRoomPassword;
            room.RoomSettings.RoomType = newRoomPassword.Length == 0 ? RoomTypes.Public : RoomTypes.Private;

            _logger.LogInformation(
                $"{roomHash} SetRoomPassword: New room password set. Authorization Token: {authorizationToken}"
            );

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetRoomPassword, newRoomPassword, room.RoomSettings.RoomType);

            IEnumerable<RoomDTO> rooms = _roomRepository.GetRoomsDTO();

            await Clients.AllExcept(Context.ConnectionId).SendAsync(HubMessages.OnListOfRoomsUpdated, JsonHelper.Serialize(rooms));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
        
    }

    [HubMethodName(HubMessages.SetUserPermissions)]
    public async Task SetUserPermissions(string roomHash, string authorizationToken, UserPermissions userPermissions)
    {
        try
        {
            Room room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetUserPermissions: Room does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetUserPermissions: User does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            if (user.IsAdmin == false)
            {
                _logger.LogInformation(
                    $"{roomHash} SetUserPermissions: User does not have the permission. Authorization Token: {authorizationToken}"
                );

                return;
            }

            room.UserPermissions = userPermissions;

            _logger.LogInformation(
                $"{roomHash} User permissions set. Authorization Token: {authorizationToken}"
            );

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetUserPermissions, JsonHelper.Serialize(userPermissions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}