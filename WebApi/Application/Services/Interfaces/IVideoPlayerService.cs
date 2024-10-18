namespace WebApi.Application.Services.Interfaces;

public interface IVideoPlayerService
{
    bool IsServiceRunning { get; }
    void StartPlaylistService(string roomHash);
}