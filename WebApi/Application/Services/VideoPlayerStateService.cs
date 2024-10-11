using WebApi.Application.Services.Interfaces;
using WebApi.Core.Entities;
using WebApi.Core.Entities.In_memory;
using System.Collections.Concurrent;

public class VideoPlayerStateService : IVideoPlayerStateService
{
    private readonly ConcurrentDictionary<string, VideoPlayer> _roomStates = new();
    private readonly ConcurrentDictionary<string, object> _roomLocks = new();

    private object GetRoomLock(string roomHash)
    {
        return _roomLocks.GetOrAdd(roomHash, new object());
    }

    public double GetCurrentTime(string roomHash)
    {
        if (_roomStates.TryGetValue(roomHash, out var videoPlayer))
        {
            lock (GetRoomLock(roomHash))
            {
                return videoPlayer.CurrentTime;
            }
        }
        else
        {
            return 0;
        }
    }

    public void SetCurrentTime(string roomHash, double currentTime)
    {
        if (_roomStates.TryGetValue(roomHash, out var videoPlayer))
        {
            lock (GetRoomLock(roomHash))
            {
                videoPlayer.CurrentTime = currentTime;
            }
        }
    }

    public bool GetIsPlaying(string roomHash)
    {
        if (_roomStates.TryGetValue(roomHash, out var videoPlayer))
        {
            lock (GetRoomLock(roomHash))
            {
                return videoPlayer.IsPlaying;
            }
        }
        else
        {
            return false;
        }
    }

    public void SetIsPlaying(string roomHash, bool isPlaying)
    {
        if (_roomStates.TryGetValue(roomHash, out var videoPlayer))
        {
            lock (GetRoomLock(roomHash))
            {
                videoPlayer.IsPlaying = isPlaying;
            }
        }
    }

    public PlaylistVideo GetCurrentVideo(string roomHash)
    {
        if (_roomStates.TryGetValue(roomHash, out var videoPlayer))
        {
            lock (GetRoomLock(roomHash))
            {
                return videoPlayer.PlaylistVideo;
            }
        }
        else
        {
            return null;
        }
    }

    public void SetCurrentVideo(string roomHash, PlaylistVideo playlistVideo)
    {
        lock (GetRoomLock(roomHash))
        {
            if (_roomStates.ContainsKey(roomHash))
            {
                _roomStates[roomHash].PlaylistVideo = playlistVideo;
            }
            else
            {
                _roomStates[roomHash] = new VideoPlayer { PlaylistVideo = playlistVideo, CurrentTime = 0, IsPlaying = false };
            }
        }
    }

    public void SetIsCurrentVideoRemoved(string roomHash, bool isRemoved)
    {
        lock (GetRoomLock(roomHash))
        {
            var state = _roomStates.GetOrAdd(roomHash, new VideoPlayer());
            state.IsCurrentVideoRemoved = isRemoved;
        }
    }

    public bool GetIsCurrentVideoRemoved(string roomHash)
    {
        if (_roomStates.TryGetValue(roomHash, out var state))
        {
            lock (GetRoomLock(roomHash))
            {
                return state.IsCurrentVideoRemoved;
            }
        }
        return false;
    }

    public VideoPlayer GetVideoPlayer(string roomHash)
    {
        if (_roomStates.TryGetValue(roomHash, out var videoPlayer))
        {
            lock (GetRoomLock(roomHash))
            {
                return videoPlayer;
            }
        }

        return null;
    }
}
