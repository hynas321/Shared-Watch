namespace WebApi.Application.Services.Interfaces;

public interface IPlaylistService
{
    bool IsServiceRunning { get; }
    Task StartPlaylistService(string roomHash);
}