using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApi.Application.Constants;
using System.Threading;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    [Authorize]
    [HubMethodName(HubMessages.SetIsVideoPlaying)]
    public async Task SetIsVideoPlaying(string roomHash, bool isPlaying)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, Context.ConnectionAborted);

        if (room is null)
        {
            _logger.LogInformation($"{roomHash} SetIsVideoPlaying: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        var role = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (role != Role.Admin && !room.UserPermissions.CanStartOrPauseVideo)
        {
            return;
        }

        _videoPlayerStateService.SetIsPlaying(roomHash, isPlaying);

        await Clients.Group(roomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, isPlaying, Context.ConnectionAborted);
    }

    [Authorize]
    [HubMethodName(HubMessages.SetPlayedSeconds)]
    public async Task SetPlayedSeconds(string roomHash, double playedSeconds)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, Context.ConnectionAborted);

        if (room is null)
        {
            _logger.LogInformation($"{roomHash} SetPlayedSeconds: Room does not exist. User identifier: {Context.UserIdentifier}");
            return;
        }

        var role = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (role != Role.Admin && !room.UserPermissions.CanSkipVideo)
        {
            return;
        }

        _videoPlayerStateService.SetCurrentTime(roomHash, playedSeconds);
    }
}