using DotnetServer.Core.Entities;
using Microsoft.AspNetCore.SignalR;

namespace DotnetServer.SignalR;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.SetIsVideoPlaying)]
    public async Task SetIsVideoPlaying(string roomHash, string authorizationToken, bool isPlaying)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetIsVideoPlaying: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} SetIsVideoPlaying: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin && !room.UserPermissions.CanStartOrPauseVideo)
            {
                _logger.LogInformation($"{roomHash} SetIsVideoPlaying: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            room.VideoPlayer.IsPlaying = isPlaying;

            await _roomRepository.UpdateRoomAsync(room);

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, isPlaying);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    [HubMethodName(HubMessages.SetPlayedSeconds)]
    public async Task SetPlayedSeconds(string roomHash, string authorizationToken, double playedSeconds)
    {
        try
        {
            Room room = await _roomRepository.GetRoomAsync(roomHash);

            if (room == null)
            {
                _logger.LogInformation($"{roomHash} SetPlayedSeconds: Room does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            User user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation($"{roomHash} SetPlayedSeconds: User does not exist. Authorization Token: {authorizationToken}");
                return;
            }

            if (!user.IsAdmin && !room.UserPermissions.CanSkipVideo)
            {
                _logger.LogInformation($"{roomHash} SetPlayedSeconds: User does not have permission. Authorization Token: {authorizationToken}");
                return;
            }

            room.VideoPlayer.SetPlayedSecondsCalled = true;
            room.VideoPlayer.CurrentTime = playedSeconds;

            await _roomRepository.UpdateRoomAsync(room);

            _logger.LogInformation($"{roomHash} SetPlayedSeconds: {playedSeconds}s. Authorization Token: {authorizationToken}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

}