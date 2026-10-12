using WebApi.Core.Entities;
using WebApi.Core.Entities.In_memory;

namespace WebApi.Application.Services;

public interface IVideoPlayerStateService
{
    double GetCurrentTime(string roomHash);
    bool SetCurrentTime(string roomHash, double currentTime);
    bool GetIsPlaying(string roomHash);
    bool SetIsPlaying(string roomHash, bool isPlaying);
    PlaylistVideo? GetCurrentVideo(string roomHash);
    void SetCurrentVideo(string roomHash, PlaylistVideo? playlistVideo);
    void SetIsCurrentVideoRemoved(string roomHash, bool isRemoved);
    bool GetIsCurrentVideoRemoved(string roomHash);
    VideoPlayer? GetVideoPlayer(string roomHash);
    void RemoveRoom(string roomHash);
}
