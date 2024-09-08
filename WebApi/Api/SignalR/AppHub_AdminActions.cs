using AutoMapper;
using WebApi.Api.DTO;
using WebApi.Core.Entities;
using WebApi.Infrastructure.Repositories;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.KickOut)]
    public async Task KickOutAsync(string roomHash, string authorizationToken, string usernameToKickOut)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = await _userRepository.GetUserByAuthorizationTokenAsync(authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin)
            {
                _logger.LogInformation($"{roomHash} KickOut: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            User userToKickOut = await _userRepository.GetUserByUsernameAsync(roomHash, usernameToKickOut);

            if (userToKickOut == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: User does not exist {usernameToKickOut}. Authorization Token: {authorizationToken}");
                return;
            }

            _logger.LogInformation($"{roomHash} KickOut: {userToKickOut.Username} {userToKickOut.IsAdmin} {userToKickOut.AuthorizationToken}. Authorization Token: {authorizationToken}");

            User kickedOutUser = await _userRepository.DeleteUserAsync(roomHash, userToKickOut.AuthorizationToken);

            if (kickedOutUser == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: Error when kicking out a user. Authorization Token: {authorizationToken}");
                return;
            }

            UserDTO kickedOutUserDTO = _mapper.Map<UserDTO>(kickedOutUser);

            await Clients.Group(roomHash).SendAsync(HubMessages.OnKickOut, JsonHelper.Serialize(kickedOutUserDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    [HubMethodName(HubMessages.SetAdminStatus)]
    public async Task SetAdminStatusAsync(string roomHash, string authorizationToken, string usernameToSetAdminStatus, bool isAdmin)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = await _userRepository.GetUserByAuthorizationTokenAsync(authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            User updatedUser = await _userRepository.GetUserByUsernameAsync(roomHash, usernameToSetAdminStatus);

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

            // Use repository methods to update the room and user
            await _roomRepository.UpdateRoomAsync(room);
            await _userRepository.UpdateUserAsync(updatedUser);

            UserDTO updatedUserDTO = _mapper.Map<UserDTO>(updatedUser);

            _logger.LogInformation($"{roomHash} SetAdminStatus: {updatedUser.Username} {updatedUser.IsAdmin} {updatedUser.AuthorizationToken}. Authorization Token: {authorizationToken}");

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetAdminStatus, JsonHelper.Serialize(updatedUserDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

}