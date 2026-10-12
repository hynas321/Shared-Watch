using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using WebApi.Application.Constants;
using WebApi.Core.Entities.In_memory;
using System.Collections.Concurrent;

namespace WebApi.SignalR;

public partial class AppHub : Hub
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> PlaybackLocks = new();

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

        var isAdmin = await IsCurrentUserAdminAsync(roomHash, Context.ConnectionAborted);

        if (!isAdmin && !room.UserPermissions.CanStartOrPauseVideo)
        {
            return;
        }

        var playbackLock = PlaybackLocks.GetOrAdd(roomHash, _ => new SemaphoreSlim(1, 1));
        await playbackLock.WaitAsync(Context.ConnectionAborted);
        try
        {
            if (!_videoPlayerStateService.SetIsPlaying(roomHash, isPlaying))
            {
                return;
            }

            await Clients.Group(roomHash).SendAsync(
                HubMessages.OnPlaybackSync,
                _videoPlayerStateService.GetVideoPlayer(roomHash),
                Context.ConnectionAborted);
        }
        finally
        {
            playbackLock.Release();
        }
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

        var isAdmin = await IsCurrentUserAdminAsync(roomHash, Context.ConnectionAborted);

        if (!isAdmin && !room.UserPermissions.CanSkipVideo)
        {
            return;
        }

        if (!double.IsFinite(playedSeconds) || playedSeconds < 0)
        {
            return;
        }

        var playbackLock = PlaybackLocks.GetOrAdd(roomHash, _ => new SemaphoreSlim(1, 1));
        await playbackLock.WaitAsync(Context.ConnectionAborted);
        try
        {
            if (!_videoPlayerStateService.SetCurrentTime(roomHash, playedSeconds))
            {
                return;
            }

            await Clients.Group(roomHash).SendAsync(
                HubMessages.OnPlaybackSync,
                _videoPlayerStateService.GetVideoPlayer(roomHash),
                Context.ConnectionAborted);
        }
        finally
        {
            playbackLock.Release();
        }
    }

    [Authorize]
    [HubMethodName(HubMessages.GetPlaybackSnapshot)]
    public async Task<VideoPlayer?> GetPlaybackSnapshot(string roomHash)
    {
        var room = await _roomRepository.GetRoomAsync(roomHash, Context.ConnectionAborted);
        return room is null ? null : _videoPlayerStateService.GetVideoPlayer(roomHash);
    }

    [Authorize]
    [HubMethodName(HubMessages.NotifyVideoEnded)]
    public async Task NotifyVideoEnded(string roomHash, string videoHash)
    {
        var cancellationToken = Context.ConnectionAborted;
        var room = await _roomRepository.GetRoomAsync(roomHash, cancellationToken);
        if (room is null)
        {
            return;
        }

        var isAdmin = await IsCurrentUserAdminAsync(roomHash, cancellationToken);
        if (!isAdmin && !room.UserPermissions.CanSkipVideo)
        {
            return;
        }

        var currentVideo = _videoPlayerStateService.GetCurrentVideo(roomHash);
        if (currentVideo?.Hash != videoHash)
        {
            return;
        }

        var removedVideo = await _playlistService.RemovePlaylistVideoAsync(roomHash, videoHash, cancellationToken);
        if (removedVideo is null)
        {
            return;
        }

        _videoPlayerStateService.SetIsCurrentVideoRemoved(roomHash, true);
        _videoPlayerStateService.SetCurrentVideo(roomHash, null);
        await Clients.Group(roomHash).SendAsync(
            HubMessages.OnPlaybackSync,
            _videoPlayerStateService.GetVideoPlayer(roomHash),
            cancellationToken);
    }
}
