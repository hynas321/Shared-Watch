namespace DotnetServer.Application.Services.Interfaces;

public interface IPlaylistService
{
    bool IsServiceRunning { get; }
    void StartPlaylistService(string roomHash);
}