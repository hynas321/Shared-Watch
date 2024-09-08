namespace WebApi.Application.Services.Interfaces;

public interface IPlaylistService
{
    bool IsServiceRunning { get; }
    void StartPlaylistService(string roomHash);
}