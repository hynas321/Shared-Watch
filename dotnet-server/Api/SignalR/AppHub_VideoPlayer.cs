using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class AppHub : Hub
{
    [HubMethodName(HubMessages.SetIsVideoPlaying)]
    public async Task SetIsVideoPlaying(string roomHash, string authorizationToken, bool isPlaying)
    {
        try
        {
            IRoom room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetIsVideoPlaying: Room does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            IUser user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetIsVideoPlaying: User does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            if (user.IsAdmin == false && room.UserPermissions.CanStartOrPauseVideo == false)
            {
                _logger.LogInformation(
                    $"{roomHash} SetIsVideoPlaying: User does not have the permission. Authorization Token: {authorizationToken}"
                );

                return;
            }

            _logger.LogInformation(
                $"{roomHash} SetIsVideoPlaying: {isPlaying}. Authorization Token: {authorizationToken}"
            );

            room.VideoPlayerState.IsPlaying = isPlaying;

            await Clients.Group(roomHash).SendAsync(HubMessages.OnSetIsVideoPlaying, isPlaying);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }

    [HubMethodName(HubMessages.SetPlayedSeconds)]
    public void SetPlayedSeconds(string roomHash, string authorizationToken, double playedSeconds)
    {
        try
        {
            IRoom room = _roomRepository.GetRoom(roomHash);

            if (room == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetPlayedSeconds: Room does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            IUser user = room.Users.FirstOrDefault(x => x.AuthorizationToken == authorizationToken);

            if (user == null)
            {
                _logger.LogInformation(
                    $"{roomHash} SetPlayedSeconds: User does not exist. Authorization Token: {authorizationToken}"
                );

                return;
            }

            if (user.IsAdmin == false && room.UserPermissions.CanSkipVideo == false)
            {
                _logger.LogInformation(
                    $"{roomHash} SetPlayedSeconds: User does not have the permission. Authorization Token: {authorizationToken}"
                );

                return;
            }
            
            room.VideoPlayerState.SetPlayedSecondsCalled = true;
            room.VideoPlayerState.CurrentTime = playedSeconds;

            _logger.LogInformation(
                $"{roomHash} SetPlayedSeconds: {playedSeconds}s. Authorization Token: {authorizationToken}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
    }
}