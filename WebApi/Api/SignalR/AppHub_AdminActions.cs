using WebApi.Api.DTO;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Application.Constants;
using WebApi.Api.SignalR;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [Authorize(Roles = Role.Admin)]
    [HubMethodName(HubMessages.KickOut)]
    public async Task KickOutAsync(string roomHash, string usernameToKickOut)
    {
        try
        {
            var room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: Room does not exist. User identifier: {Context.UserIdentifier}");
                return;
            }

            var userToKickOut = await _userRepository.GetUserAsync(roomHash, usernameToKickOut);

            if (userToKickOut == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: User does not exist {usernameToKickOut}. User identifier: {Context.UserIdentifier}");
                return;
            }

            _logger.LogInformation($"{roomHash} KickOut: {userToKickOut.Username}. User identifier: {Context.UserIdentifier}");

            var connectionId = _hubConnectionMapper.GetConnectionIdByUserId(usernameToKickOut);

            var kickedOutUser = await _userRepository.DeleteUserByConnectionIdAsync(roomHash, connectionId);

            if (kickedOutUser == null)
            {
                _logger.LogInformation($"{roomHash} KickOut: Error when kicking out a user. User identifier: {connectionId}");
                return;
            }

            var kickedOutUserDTO = _mapper.Map<UserDTO>(kickedOutUser);

            await Clients.Group(roomHash).SendAsync(HubMessages.OnKickOut, JsonHelper.Serialize(kickedOutUserDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    [Authorize(Roles = Role.Admin)]
    [HubMethodName(HubMessages.SetAdminStatus)]
    public async Task SetAdminStatusAsync(string roomHash, string usernameToSetAdminStatus, bool isAdmin)
    {
        try
        {
            var room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: Room does not exist. User identifier: {Context.UserIdentifier}");
                return;
            }

            var updatedUser = await _userRepository.GetUserAsync(roomHash, usernameToSetAdminStatus);

            if (updatedUser == null)
            {
                _logger.LogInformation($"{roomHash} SetAdminStatus: User does not exist: {usernameToSetAdminStatus}. User identifier: {Context.UserIdentifier}");
                return;
            }

            updatedUser.Role = isAdmin ? Role.Admin : Role.User;

            await _userRepository.UpdateUserAsync(updatedUser);

            var updatedUserDTO = _mapper.Map<UserDTO>(updatedUser);

            var newJwtToken = _jwtTokenService.GenerateToken(
                updatedUser.Username,
                isAdmin ? Role.Admin : Role.User,
                roomHash);

            var connectionId = _hubConnectionMapper.GetConnectionIdByUserId(usernameToSetAdminStatus);

            await Clients.Client(connectionId).SendAsync(HubMessages.OnReceiveJwt, newJwtToken);
            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetAdminStatus, JsonHelper.Serialize(updatedUserDTO));

            _logger.LogInformation($"{roomHash} SetAdminStatus: {updatedUser.Username}. Role: {updatedUser.Role} User identifier: {Context.UserIdentifier}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

}