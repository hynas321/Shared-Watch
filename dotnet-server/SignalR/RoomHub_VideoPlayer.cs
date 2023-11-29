using Microsoft.AspNetCore.SignalR;

namespace Dotnet.Server.Hubs;

public partial class RoomHub : Hub
{
    [HubMethodName(HubEvents.SetIsVideoPlaying)]
    public async Task SetIsVideoPlaying(string roomHash, string authorizationToken, bool isPlaying)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} SetIsVideoPlaying: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} SetIsVideoPlaying: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false && room.UserPermissions.canStartOrPauseVideo == false)
        {
            _logger.LogInformation($"{roomHash} SetIsVideoPlaying: User does not have the permission. Authorization Token: {authorizationToken}");
            return;
        }

        _logger.LogInformation($"{roomHash} SetIsVideoPlaying: {isPlaying}. Authorization Token: {authorizationToken}");

        room.VideoPlayerState.IsPlaying = isPlaying;

        await Clients.Group(roomHash).SendAsync(HubEvents.OnSetIsVideoPlaying, isPlaying);
    }

    [HubMethodName(HubEvents.SetPlayedSeconds)]
    public async Task SetPlayedSeconds(string roomHash, string authorizationToken, double playedSeconds)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} SetPlayedSeconds: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} SetPlayedSeconds: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        if (user.IsAdmin == false && room.UserPermissions.canSkipVideo == false)
        {
            _logger.LogInformation($"{roomHash} SetPlayedSeconds: User does not have the permission. Authorization Token: {authorizationToken}");
            return;
        }        

        room.VideoPlayerState.CurrentTime = playedSeconds;

        _logger.LogInformation($"{roomHash} SetPlayedSeconds: {playedSeconds}s. Authorization Token: {authorizationToken}");

        await Clients.Group(roomHash).SendAsync(HubEvents.OnSetPlayedSeconds, playedSeconds);
    }

    [HubMethodName(HubEvents.GetVideoDuration)]
    public void GetVideoDuration(string roomHash, string authorizationToken, double duration)
    {
        Room room = _roomManager.GetRoom(roomHash);

        if (room == null)
        {
            _logger.LogInformation($"{roomHash} GetVideoDuration: Room does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        User user = room.GetUser(authorizationToken);

        if (user == null)
        {
            _logger.LogInformation($"{roomHash} GetVideoDuration: User does not exist. Authorization Token: {authorizationToken}");
            return;
        }

        room.VideoPlayerState.Duration = duration;

        _logger.LogInformation($"{roomHash} GetVideoDuration: {duration}s. Authorization Token: {authorizationToken}");

    }
}