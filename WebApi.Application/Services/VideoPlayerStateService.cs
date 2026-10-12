using System.Collections.Concurrent;
using WebApi.Application.Services;
using WebApi.Core.Entities;
using WebApi.Core.Entities.In_memory;

namespace WebApi.Application.Services;

public sealed class VideoPlayerStateService : IVideoPlayerStateService
{
    private readonly ConcurrentDictionary<string, VideoPlayer> _roomStates = new();
    private readonly ConcurrentDictionary<string, object> _roomLocks = new();
    private object GetRoomLock(string roomHash) => _roomLocks.GetOrAdd(roomHash, static _ => new object());

    public double GetCurrentTime(string roomHash)
    {
        if (!_roomStates.TryGetValue(roomHash, out var state)) return 0;
        lock (GetRoomLock(roomHash)) return GetEffectiveTime(state, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    public bool SetCurrentTime(string roomHash, double currentTime)
    {
        var state = _roomStates.GetOrAdd(roomHash, static _ => new VideoPlayer());
        lock (GetRoomLock(roomHash))
        {
            state.CurrentTime = Math.Max(0, currentTime);
            Touch(state);
        }
        return true;
    }

    public bool GetIsPlaying(string roomHash)
    {
        if (!_roomStates.TryGetValue(roomHash, out var state)) return false;
        lock (GetRoomLock(roomHash)) return state.IsPlaying;
    }

    public bool SetIsPlaying(string roomHash, bool isPlaying)
    {
        var state = _roomStates.GetOrAdd(roomHash, static _ => new VideoPlayer());
        lock (GetRoomLock(roomHash))
        {
            state.CurrentTime = GetEffectiveTime(state, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            state.IsPlaying = isPlaying;
            Touch(state);
        }
        return true;
    }

    public PlaylistVideo? GetCurrentVideo(string roomHash)
    {
        if (!_roomStates.TryGetValue(roomHash, out var state)) return null;
        lock (GetRoomLock(roomHash)) return state.PlaylistVideo;
    }

    public void SetCurrentVideo(string roomHash, PlaylistVideo? playlistVideo)
    {
        lock (GetRoomLock(roomHash))
        {
            var state = _roomStates.GetOrAdd(roomHash, static _ => new VideoPlayer());
            state.PlaylistVideo = playlistVideo;
            state.CurrentTime = 0;
            state.IsPlaying = false;
            state.IsCurrentVideoRemoved = false;
            Touch(state);
        }
    }

    public void SetIsCurrentVideoRemoved(string roomHash, bool isRemoved)
    {
        var state = _roomStates.GetOrAdd(roomHash, static _ => new VideoPlayer());
        lock (GetRoomLock(roomHash))
        {
            state.IsCurrentVideoRemoved = isRemoved;
            Touch(state);
        }
    }

    public bool GetIsCurrentVideoRemoved(string roomHash)
    {
        if (!_roomStates.TryGetValue(roomHash, out var state)) return false;
        lock (GetRoomLock(roomHash)) return state.IsCurrentVideoRemoved;
    }

    public VideoPlayer? GetVideoPlayer(string roomHash)
    {
        if (!_roomStates.TryGetValue(roomHash, out var state)) return null;
        lock (GetRoomLock(roomHash))
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return new VideoPlayer
            {
                IsPlaying = state.IsPlaying,
                PlaylistVideo = state.PlaylistVideo,
                IsCurrentVideoRemoved = state.IsCurrentVideoRemoved,
                CurrentTime = GetEffectiveTime(state, now),
                ServerTimestampUnixMs = now,
                Revision = state.Revision
            };
        }
    }

    public void RemoveRoom(string roomHash)
    {
        _roomStates.TryRemove(roomHash, out _);
        _roomLocks.TryRemove(roomHash, out _);
    }

    private static void Touch(VideoPlayer state)
    {
        state.ServerTimestampUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        state.Revision++;
    }

    private static double GetEffectiveTime(VideoPlayer state, long now)
    {
        if (!state.IsPlaying)
        {
            return state.CurrentTime;
        }

        return state.CurrentTime + Math.Max(0, now - state.ServerTimestampUnixMs) / 1000d;
    }
}
