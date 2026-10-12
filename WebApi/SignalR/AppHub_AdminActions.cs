using WebApi.Application.DTO;
using WebApi.Application.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Application.Constants;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.KickOut)]
    public async Task KickOutAsync(string roomHash, string usernameToKickOut)
    {
        var cancellationToken = Context.ConnectionAborted;
        if (!await IsCurrentUserAdminAsync(roomHash, cancellationToken))
        {
            _logger.LogInformation($"{roomHash} KickOut: User is not authorized. User identifier: {Context.UserIdentifier}");
            return;
        }

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

        var connectionId = _hubConnectionRegistry.GetConnectionIdsByUserId(roomHash, usernameToKickOut).FirstOrDefault();
        if (connectionId is null)
        {
            return;
        }

        var kickedOutUser = await _userRepository.DeleteUserByUsernameAsync(roomHash, usernameToKickOut, cancellationToken);

        if (kickedOutUser is null)
        {
            _logger.LogInformation($"{roomHash} KickOut: Error when kicking out a user. User identifier: {connectionId}");
            return;
        }

        var kickedOutUserDTO = UserDTO.FromEntity(kickedOutUser);

        await Clients.Group(roomHash).SendAsync(HubMessages.OnKickOut, kickedOutUserDTO, Context.ConnectionAborted);
    }

    [HubMethodName(HubMessages.SetAdminStatus)]
    public async Task SetAdminStatusAsync(string roomHash, string usernameToSetAdminStatus, bool isAdmin)
    {

        var cancellationToken = Context.ConnectionAborted;
        if (!await IsCurrentUserAdminAsync(roomHash, cancellationToken))
        {
            _logger.LogInformation($"{roomHash} SetAdminStatus: User is not authorized. User identifier: {Context.UserIdentifier}");
            return;
        }

        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);

        if (room is null)
        {
            _logger.LogInformation($"{roomHash} SetAdminStatus: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        var updatedUser = await _userRepository.GetUserAsync(roomHash, usernameToSetAdminStatus, cancellationToken);

        if (updatedUser is null)
        {
            _logger.LogInformation($"{roomHash} SetAdminStatus: User does not exist: {usernameToSetAdminStatus}. User identifier: {Context.UserIdentifier}");
            return;
        }

        updatedUser.Role = isAdmin ? Role.Admin : Role.User;

        await _userRepository.UpdateUserAsync(updatedUser, cancellationToken);

        var updatedUserDTO = UserDTO.FromEntity(updatedUser);

        await Clients.Group(roomHash).SendAsync(HubMessages.OnSetAdminStatus, updatedUserDTO, cancellationToken);

        var connectionIds = _hubConnectionRegistry.GetConnectionIdsByUserId(roomHash, usernameToSetAdminStatus);
        foreach (var connectionId in connectionIds)
        {
            await Clients.Client(connectionId).SendAsync(HubMessages.OnReceiveJwt, cancellationToken);
        }

        _logger.LogInformation($"{roomHash} SetAdminStatus: {updatedUser.Username}. Role: {updatedUser.Role} User identifier: {Context.UserIdentifier}");
    }

}
