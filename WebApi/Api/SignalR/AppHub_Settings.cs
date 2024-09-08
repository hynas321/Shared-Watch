using WebApi.Api.DTO;
using WebApi.Core.Entities;
using WebApi.Core.Enums;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.SetRoomPassword)]
    public async Task SetRoomPassword(string roomHash, string authorizationToken, string newRoomPassword)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetRoomPassword: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} SetRoomPassword: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin)
            {
                _logger.LogInformation($"{roomHash} SetRoomPassword: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            room.RoomSettings.RoomPassword = newRoomPassword;
            room.RoomSettings.RoomType = string.IsNullOrEmpty(newRoomPassword) ? RoomTypes.Public : RoomTypes.Private;

            await _roomRepository.UpdateRoomAsync(room);

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetRoomPassword, newRoomPassword, room.RoomSettings.RoomType);

            IEnumerable<RoomDTO> rooms = await _roomRepository.GetRoomsDTOAsync();

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
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetUserPermissions: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} SetUserPermissions: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin)
            {
                _logger.LogInformation($"{roomHash} SetUserPermissions: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            room.UserPermissions = userPermissions;

            await _roomRepository.UpdateRoomAsync(room);

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetUserPermissions, JsonHelper.Serialize(userPermissions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}