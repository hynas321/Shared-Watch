using WebApi.Core.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApi.Application.Constants;
using WebApi.Application.Services;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [Authorize]
    [HubMethodName(HubMessages.SetIsVideoPlaying)]
    public async Task SetIsVideoPlaying(string roomHash, bool isPlaying)
    {
        try
        {
            var room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetIsVideoPlaying: Room does not exist. User identifier: {Context.UserIdentifier}");
                return;
            }

            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (role != Role.Admin && !room.UserPermissions.CanStartOrPauseVideo)
            {
                _logger.LogInformation($"{roomHash} SetIsVideoPlaying: User does not have permission. User identifier: {Context.UserIdentifier}");
                return;
            }

            _videoPlayerStateService.SetIsPlaying(roomHash, isPlaying);

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, isPlaying);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    [Authorize]
    [HubMethodName(HubMessages.SetPlayedSeconds)]
    public async Task SetPlayedSeconds(string roomHash, double playedSeconds)
    {
        try
        {
            var room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetPlayedSeconds: Room does not exist. User identifier: {Context.UserIdentifier}");
                return;
            }

            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (role != Role.Admin && !room.UserPermissions.CanSkipVideo)
            {
                _logger.LogInformation($"{roomHash} SetPlayedSeconds: User does not have permission. User identifier: {Context.UserIdentifier}");
                return;
            }

            _videoPlayerStateService.SetCurrentTime(roomHash, playedSeconds);

            _logger.LogInformation($"{roomHash} SetPlayedSeconds: {playedSeconds}s. User identifier: {Context.UserIdentifier}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}