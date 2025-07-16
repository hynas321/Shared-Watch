using WebApi.Api.DTO;
using WebApi.Shared.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Application.Constants;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [Authorize(Roles = Role.Admin)]
    [HubMethodName(HubMessages.KickOut)]
    public async Task KickOutAsync(string roomHash, string usernameToKickOut)
    {
        var cancellationToken = Context.ConnectionAborted;

        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
        {
            _logger.LogInformation($"{roomHash} KickOut: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        var userToKickOut = await _userRepository.GetUserAsync(roomHash, usernameToKickOut, cancellationToken);

        if (userToKickOut is null)
        {
            _logger.LogInformation($"{roomHash} KickOut: User does not exist {usernameToKickOut}. User identifier: {Context.UserIdentifier}");
            return;
        }

        _logger.LogInformation($"{roomHash} KickOut: {userToKickOut.Username}. User identifier: {Context.UserIdentifier}");

        var connectionId = _hubConnectionMapper.GetConnectionIdsByUserId(usernameToKickOut).First();

        var kickedOutUser = await _userRepository.DeleteUserByConnectionIdAsync(roomHash, connectionId, cancellationToken);

        if (kickedOutUser is null)
        {
            _logger.LogInformation($"{roomHash} KickOut: Error when kicking out a user. User identifier: {connectionId}");
            return;
        }

        var kickedOutUserDTO = _mapper.Map<UserDTO>(kickedOutUser);

        await Clients.Group(roomHash).SendAsync(HubMessages.OnKickOut, JsonHelper.Serialize(kickedOutUserDTO), Context.ConnectionAborted);
    }

    [Authorize(Roles = Role.Admin)]
    [HubMethodName(HubMessages.SetAdminStatus)]
    public async Task SetAdminStatusAsync(string roomHash, string usernameToSetAdminStatus, bool isAdmin)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, Context.ConnectionAborted);

        if (room is null)
        {
            _logger.LogInformation($"{roomHash} SetAdminStatus: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        var updatedUser = await _userRepository.GetUserAsync(roomHash, usernameToSetAdminStatus, Context.ConnectionAborted);

        if (updatedUser is null)
        {
            _logger.LogInformation($"{roomHash} SetAdminStatus: User does not exist: {usernameToSetAdminStatus}. User identifier: {Context.UserIdentifier}");
            return;
        }

        updatedUser.Role = isAdmin ? Role.Admin : Role.User;

        await _userRepository.UpdateUserAsync(updatedUser, Context.ConnectionAborted);

        var updatedUserDTO = _mapper.Map<UserDTO>(updatedUser);

        var newJwtToken = _jwtTokenService.GenerateToken(
            updatedUser.Username,
            isAdmin ? Role.Admin : Role.User,
            roomHash);

        var connectionId = _hubConnectionMapper.GetConnectionIdsByUserId(usernameToSetAdminStatus).First();

        await Clients.Client(connectionId).SendAsync(HubMessages.OnReceiveJwt, newJwtToken, Context.ConnectionAborted);
        await Clients.Group(roomHash).SendAsync(HubMessages.OnSetAdminStatus, JsonHelper.Serialize(updatedUserDTO), Context.ConnectionAborted);

        _logger.LogInformation($"{roomHash} SetAdminStatus: {updatedUser.Username}. Role: {updatedUser.Role} User identifier: {Context.UserIdentifier}");
    }

}